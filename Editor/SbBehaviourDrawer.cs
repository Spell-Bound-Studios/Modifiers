// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Editor {
    /// <summary>
    /// Default UI Toolkit drawer for any <see cref="SbBehaviour"/> subclass. When the behaviour declares owned
    /// stats (<see cref="SbBehaviour.Declare"/>) it reveals exactly those as pre-filled value fields — no
    /// stat-picker, no orphans, no forgetting one. Otherwise it falls back to rendering the raw stats list.
    /// A read-only computed preview follows either way.
    /// </summary>
    [CustomPropertyDrawer(typeof(SbBehaviour), true)]
    public sealed class SbBehaviourDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var root = new VisualElement {
                style = {
                    marginTop = 2
                }
            };

            var behaviour = property.managedReferenceValue as SbBehaviour;
            var declared = behaviour?.Declare();
            var hasDeclared = declared != null && declared.Count > 0;

            // Typed [SerializeField] children render normally — but hide the raw stats list when we're
            // revealing the declared stats instead (otherwise the designer sees both).
            EditorListHelpers.ForEachVisibleChild(property, child => {
                if (hasDeclared && child.name == "stats")
                    return;

                var pf = new PropertyField(child.Copy());
                pf.Bind(property.serializedObject);
                root.Add(pf);
            });

            if (hasDeclared) {
                var statsProp = property.FindPropertyRelative("stats");
                SyncStatsToDeclared(statsProp, declared);

                root.Add(EditorListHelpers.SectionHeader("Stats"));

                for (var i = 0; i < declared.Count; i++) {
                    var definition = StatRegistry.GetDefinition(declared[i].statHash);
                    var label = definition != null ? definition.DisplayName : $"#{declared[i].statHash}";
                    var valueProp = statsProp.GetArrayElementAtIndex(i).FindPropertyRelative("baseValue");

                    var field = new FloatField(label);
                    field.BindProperty(valueProp);
                    root.Add(field);
                }
            }

            root.Add(EditorListHelpers.SectionHeader("Stats (read-only preview)"));

            root.Add(EditorListHelpers.BuildLivePreview(
                property.serializedObject,
                () => ComputePreviewText(property)));

            return root;
        }

        /// <summary>
        /// Rebuilds the serialized stats list to exactly the declared stats (preserving authored values by
        /// hash, defaulting new ones), but only when it's actually out of sync — so we don't dirty the asset
        /// or spam undo on every inspector refresh.
        /// </summary>
        private static void SyncStatsToDeclared(SerializedProperty statsProp, IReadOnlyList<StatAndValue> declared) {
            var inSync = statsProp.arraySize == declared.Count;

            if (inSync) {
                for (var i = 0; i < declared.Count; i++) {
                    var stat = statsProp.GetArrayElementAtIndex(i)
                            .FindPropertyRelative("stat").objectReferenceValue as StatDefinition;

                    if (stat == null || stat.Hash != declared[i].statHash) {
                        inSync = false;

                        break;
                    }
                }
            }

            if (inSync)
                return;

            var existing = new Dictionary<uint, float>();

            for (var i = 0; i < statsProp.arraySize; i++) {
                var entry = statsProp.GetArrayElementAtIndex(i);
                var stat = entry.FindPropertyRelative("stat").objectReferenceValue as StatDefinition;

                if (stat != null)
                    existing[stat.Hash] = entry.FindPropertyRelative("baseValue").floatValue;
            }

            statsProp.arraySize = declared.Count;

            for (var i = 0; i < declared.Count; i++) {
                var entry = statsProp.GetArrayElementAtIndex(i);

                entry.FindPropertyRelative("stat").objectReferenceValue = StatRegistry.GetDefinition(declared[i].statHash);
                entry.FindPropertyRelative("baseValue").floatValue =
                        existing.TryGetValue(declared[i].statHash, out var v) ? v : declared[i].amount;
            }

            statsProp.serializedObject.ApplyModifiedProperties();
        }

        private static string ComputePreviewText(SerializedProperty property) {
            try {
                if (property.managedReferenceValue is not SbBehaviour behaviour)
                    return "(no instance)";

                ((ISerializationCallbackReceiver)behaviour).OnAfterDeserialize();

                return behaviour.StatCount == 0
                        ? "(no stats)"
                        : behaviour.GetCalculatedStatList();
            }
            catch (Exception ex) {
                return $"<preview error: {ex.Message}>";
            }
        }
    }
}
