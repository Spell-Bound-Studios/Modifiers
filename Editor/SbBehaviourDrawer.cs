// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Editor {
    /// <summary>
    /// Default UI Toolkit drawer for any <see cref="SbBehaviour"/> subclass. Renders the typed
    /// <c>[SerializeField]</c> children normally (so concrete behaviours keep their authored knobs), then
    /// appends a read-only "Stats" panel that re-syncs the behaviour from its fields and prints
    /// <see cref="SbBehaviour.GetCalculatedStatList"/>. The panel updates whenever any field on the parent
    /// serialized object changes.
    /// </summary>
    /// <remarks>
    /// Operates on the live managed-reference instance: <see cref="ISerializationCallbackReceiver.OnAfterDeserialize"/>
    /// is invoked manually before each refresh so <see cref="SbBehaviour.SyncStatsFromFields"/> runs and the
    /// preview matches what the runtime will see post-deserialize.
    /// </remarks>
    [CustomPropertyDrawer(typeof(SbBehaviour), useForChildren: true)]
    public sealed class SbBehaviourDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var root = new VisualElement {
                style = {
                    marginTop = 2
                }
            };

            // Typed fields — default rendering, honours [Header] / [Tooltip] / per-field drawers.
            EditorListHelpers.ForEachVisibleChild(property, child => {
                var pf = new PropertyField(child.Copy());
                pf.Bind(property.serializedObject);
                root.Add(pf);
            });

            root.Add(EditorListHelpers.SectionHeader("Stats (read-only preview)"));
            root.Add(EditorListHelpers.BuildLivePreview(
                property.serializedObject,
                () => ComputePreviewText(property)));

            return root;
        }

        private static string ComputePreviewText(SerializedProperty property) {
            try {
                if (property.managedReferenceValue is not SbBehaviour behaviour)
                    return "(no instance)";

                // Push the just-edited field values into _baseValues by running the same hook that fires
                // on deserialize at runtime. Editor field-change events don't trigger deserialize, so we do it.
                ((ISerializationCallbackReceiver)behaviour).OnAfterDeserialize();

                return behaviour.StatCount == 0
                        ? "(no stats — override SyncStatsFromFields to seed base values)"
                        : behaviour.GetCalculatedStatList();
            }
            catch (Exception ex) {
                return $"<preview error: {ex.Message}>";
            }
        }
    }
}
