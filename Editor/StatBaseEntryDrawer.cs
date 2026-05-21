// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Editor {
    /// <summary>
    /// Inline row for a <see cref="StatBaseEntry"/>: a searchable <see cref="StatDefinition"/> picker on the
    /// left and a float input for the base value on the right. The picker pops an <c>AdvancedDropdown</c>
    /// listing every <see cref="StatDefinition"/> asset in the project so designers can type-to-filter
    /// instead of scrubbing the project view.
    /// </summary>
    [CustomPropertyDrawer(typeof(StatBaseEntry))]
    public sealed class StatBaseEntryDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var statProp = property.FindPropertyRelative("stat");
            var baseValueProp = property.FindPropertyRelative("baseValue");

            var row = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 2
                }
            };

            var pickerButton = new Button {
                text = FormatStatLabel(statProp.objectReferenceValue as StatDefinition),
                style = {
                    flexGrow = 1,
                    marginRight = 4,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    paddingLeft = 8
                }
            };

            pickerButton.clicked += () => {
                var alreadyUsed = CollectSiblingStats(property);

                ShowStatPicker(pickerButton.worldBound, alreadyUsed, picked => {
                    statProp.objectReferenceValue = picked;
                    statProp.serializedObject.ApplyModifiedProperties();
                    pickerButton.text = FormatStatLabel(picked);
                });
            };

            var valueField = new FloatField {
                value = baseValueProp.floatValue,
                style = {
                    width = 80
                }
            };

            valueField.RegisterValueChangedCallback(evt => {
                baseValueProp.floatValue = evt.newValue;
                baseValueProp.serializedObject.ApplyModifiedProperties();
            });

            valueField.TrackPropertyValue(baseValueProp, p => valueField.SetValueWithoutNotify(p.floatValue));
            pickerButton.TrackPropertyValue(statProp,
                p => pickerButton.text = FormatStatLabel(p.objectReferenceValue as StatDefinition));

            row.Add(pickerButton);
            row.Add(valueField);

            return row;
        }

        private static string FormatStatLabel(StatDefinition def) {
            if (def == null)
                return "(no stat — click to pick)";

            var displayName = string.IsNullOrEmpty(def.DisplayName) ? def.StatName : def.DisplayName;

            return string.IsNullOrEmpty(displayName) ? def.name : displayName;
        }

        private static void ShowStatPicker(
            Rect anchor, HashSet<StatDefinition> exclude, Action<StatDefinition> onPicked) {
            var dropdown = new StatDefinitionAdvancedDropdown(new AdvancedDropdownState(), exclude, onPicked);
            dropdown.Show(anchor);
        }

        /// <summary>
        /// Walk the parent list this entry belongs to and collect every <see cref="StatDefinition"/> already
        /// referenced by a sibling entry. Used to filter the picker so the same stat can't be selected on
        /// two different rows.
        /// </summary>
        private static HashSet<StatDefinition> CollectSiblingStats(SerializedProperty elementProperty) {
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

            // Extract this element's index so we don't exclude our own current selection.
            var bracketStart = path.LastIndexOf('[') + 1;
            var bracketEnd = path.LastIndexOf(']');

            if (!int.TryParse(path[bracketStart..bracketEnd], out var selfIndex))
                selfIndex = -1;

            for (var i = 0; i < listProp.arraySize; i++) {
                if (i == selfIndex)
                    continue;

                var entry = listProp.GetArrayElementAtIndex(i);
                var siblingStat = entry.FindPropertyRelative("stat").objectReferenceValue as StatDefinition;

                if (siblingStat != null)
                    used.Add(siblingStat);
            }

            return used;
        }

        // ============================================================================================
        // AdvancedDropdown — searchable, scrollable picker scanning every StatDefinition in the project
        // ============================================================================================

        private sealed class StatDefinitionAdvancedDropdown : AdvancedDropdown {
            private readonly Action<StatDefinition> _onPicked;
            private readonly List<StatDefinition> _candidates;
            private readonly HashSet<StatDefinition> _exclude;

            public StatDefinitionAdvancedDropdown(
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

            // Exclude the lib's own in-place samples folder. Samples imported via Package Manager land at
            // Assets/Samples/<package>/<version>/<sample-name>/ which does NOT match this path — so a user
            // who installs the samples that way still sees them in the picker. The lib developer (working
            // in the multi-repo clone layout) gets a clean picker without sample noise.
            private const string LibSamplesPathFragment = "/Modifiers/Samples/";

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
