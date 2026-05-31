// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Editor {
    /// <summary>
    /// Shared <c>AdvancedDropdown</c>-based <see cref="StatDefinition"/> picker. Every drawer that needs a
    /// searchable stat-asset chooser opens it via <see cref="Open"/>. Scans every <see cref="StatDefinition"/>
    /// asset in the project, hides the lib's own in-place samples (so the lib developer's picker stays
    /// clean), respects an optional exclusion set (so a drawer can hide stats already used elsewhere in the
    /// same list), and supports a "(none)" selection that clears the field.
    /// </summary>
    internal static class StatDefinitionPicker {
        // Exclude the lib's own in-place samples folder. Samples imported via Package Manager land at
        // Assets/Samples/<package>/<version>/<sample-name>/ which does NOT match this path — so a user
        // who installs the samples that way still sees them in the picker. The lib developer (working
        // in the multi-repo clone layout) gets a clean picker without sample noise.
        private const string LibSamplesPathFragment = "/Modifiers/Samples/";

        /// <summary>
        /// Open the picker anchored to <paramref name="anchorElement"/>; on selection, invoke
        /// <paramref name="onPicked"/>. Stats present in <paramref name="exclude"/> are filtered out (used for
        /// sibling-deduplication).
        /// </summary>
        /// <remarks>
        /// Invoked synchronously from a UI Toolkit <c>Button.clicked</c> callback on Unity 6,
        /// <see cref="AdvancedDropdown.Show"/> emits <c>Unable to find style 'DD ItemStyle' in skin 'GameSkin'</c>
        /// (and friends) and renders every item in the missing-style fallback color — because the call
        /// originates outside any active IMGUI frame where <c>GUI.skin</c> resolves to the editor skin.
        /// <para>
        /// Deferring with <see cref="EditorApplication.delayCall"/> makes it worse: the deferred lambda runs
        /// <c>&lt;called outside OnGUI&gt;</c> entirely, AND the screen-space conversion of the anchor Rect
        /// has no GUI context to convert against, so the popup spawns at the wrong screen position.
        /// </para>
        /// <para>
        /// The fix is to invoke <c>Show</c> from inside a real OnGUI tick. We attach a hidden
        /// <see cref="IMGUIContainer"/> to the anchor's panel root; its <c>onGUIHandler</c> runs in a normal
        /// IMGUI editor frame (editor skin active, anchor Rect convertible), fires <c>Show</c> once with a
        /// freshly-read <see cref="VisualElement.worldBound"/>, then detaches itself.
        /// </para>
        /// </remarks>
        public static void Open(
            VisualElement anchorElement, HashSet<StatDefinition> exclude, Action<StatDefinition> onPicked) {
            if (anchorElement?.panel == null) {
                Spellbound.Core.Logging.Log.Warn(
                    "[StatDefinitionPicker] Anchor element has no panel; dropdown cannot open.");

                return;
            }

            var dropdown = new Dropdown(new AdvancedDropdownState(), exclude, onPicked);
            var root = anchorElement.panel.visualTree;

            IMGUIContainer bridge = null;
            var fired = false;

            bridge = new IMGUIContainer(() => {
                if (fired)
                    return;

                fired = true;
                dropdown.Show(anchorElement.worldBound);

                // Detach next frame so we leave the OnGUI tick cleanly before mutating the tree.
                var toRemove = bridge;
                anchorElement.schedule.Execute(() => toRemove?.RemoveFromHierarchy());
            }) {
                // Zero-sized + not picking-target so the bridge is invisible and click-transparent.
                style = {
                    width = 0,
                    height = 0,
                    position = Position.Absolute
                },
                pickingMode = PickingMode.Ignore
            };

            root.Add(bridge);
        }

        /// <summary>
        /// Format a <see cref="StatDefinition"/> as a designer-friendly button label — prefers
        /// <see cref="StatDefinition.DisplayName"/>, falls back to <see cref="StatDefinition.StatName"/>,
        /// then to the asset's Unity name. Returns a "(no stat — click to pick)" hint when null.
        /// </summary>
        public static string FormatLabel(StatDefinition def) {
            if (def == null)
                return "(no stat — click to pick)";

            var displayName = string.IsNullOrEmpty(def.DisplayName) ? def.StatName : def.DisplayName;

            return string.IsNullOrEmpty(displayName) ? def.name : displayName;
        }

        /// <summary>
        /// Walk the parent list of <paramref name="elementProperty"/> and collect every
        /// <see cref="StatDefinition"/> referenced by sibling entries (excluding the element itself, so its
        /// own current selection stays pickable). Used to filter the picker so the same stat can't be
        /// selected on two different rows of the same list. <paramref name="statFieldName"/> is the name of
        /// the <see cref="StatDefinition"/> field on the element struct — pass via <c>nameof(...)</c>.
        /// </summary>
        public static HashSet<StatDefinition> CollectSiblings(
            SerializedProperty elementProperty, string statFieldName) {
            var used = new HashSet<StatDefinition>();
            var path = elementProperty.propertyPath;

            const string arrayMarker = ".Array.data[";
            var arrayIdx = path.LastIndexOf(arrayMarker, StringComparison.Ordinal);

            if (arrayIdx < 0)
                return used;

            var listPath = path[..arrayIdx];
            var listProp = elementProperty.serializedObject.FindProperty(listPath);

            if (listProp is not { isArray: true })
                return used;

            var bracketStart = path.LastIndexOf('[') + 1;
            var bracketEnd = path.LastIndexOf(']');

            if (!int.TryParse(path[bracketStart..bracketEnd], out var selfIndex))
                selfIndex = -1;

            for (var i = 0; i < listProp.arraySize; i++) {
                if (i == selfIndex)
                    continue;

                var entry = listProp.GetArrayElementAtIndex(i);
                var siblingStat = entry.FindPropertyRelative(statFieldName)?.objectReferenceValue as StatDefinition;

                if (siblingStat != null)
                    used.Add(siblingStat);
            }

            return used;
        }

        // ============================================================================================
        // Internal AdvancedDropdown implementation
        // ============================================================================================

        private sealed class Dropdown : AdvancedDropdown {
            private readonly Action<StatDefinition> _onPicked;
            private readonly List<StatDefinition> _candidates;
            private readonly HashSet<StatDefinition> _exclude;

            public Dropdown(
                AdvancedDropdownState state, HashSet<StatDefinition> exclude, Action<StatDefinition> onPicked)
                    : base(state) {
                _onPicked = onPicked;
                _exclude = exclude;
                _candidates = LoadAllStatDefinitions();
                minimumSize = new Vector2(260, 320);
            }

            protected override AdvancedDropdownItem BuildRoot() {
                var root = new AdvancedDropdownItem("Stat Definitions");

                root.AddChild(new NullItem());

                var visibleCount = 0;

                for (var i = 0; i < _candidates.Count; i++) {
                    var def = _candidates[i];

                    if (_exclude != null && _exclude.Contains(def))
                        continue;

                    var label = string.IsNullOrEmpty(def.DisplayName) ? def.StatName : def.DisplayName;

                    if (string.IsNullOrEmpty(label))
                        label = def.name;

                    root.AddChild(new CandidateItem(label, i));
                    visibleCount++;
                }

                if (visibleCount == 0) {
                    root.AddChild(new AdvancedDropdownItem("(every stat is already used on this list)") {
                        enabled = false
                    });
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item) {
                switch (item) {
                    case NullItem:
                        _onPicked(null);

                        break;
                    case CandidateItem c:
                        _onPicked(_candidates[c.Index]);

                        break;
                }
            }

            private static List<StatDefinition> LoadAllStatDefinitions() {
                var guids = AssetDatabase.FindAssets($"t:{nameof(StatDefinition)}");

                return guids
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .Where(path => path.IndexOf(LibSamplesPathFragment, StringComparison.OrdinalIgnoreCase) < 0)
                        .Select(AssetDatabase.LoadAssetAtPath<StatDefinition>)
                        .Where(d => d != null)
                        .OrderBy(d => string.IsNullOrEmpty(d.DisplayName) ? d.StatName : d.DisplayName)
                        .ToList();
            }

            private sealed class NullItem : AdvancedDropdownItem {
                public NullItem() : base("(none)") { }
            }

            private sealed class CandidateItem : AdvancedDropdownItem {
                public int Index { get; }

                public CandidateItem(string label, int index) : base(label) {
                    Index = index;
                }
            }
        }
    }
}