// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Editor {
    /// <summary>
    /// Shared UI Toolkit inspector for every <see cref="PipelineConfig{TContext}"/>. Renders a reorderable list
    /// where list-order = execution-order, with a per-row type picker filtered to the
    /// <see cref="PipelineStageRegistry"/> entries that match this config's context type.
    /// </summary>
    public abstract class PipelineConfigEditorBase : UnityEditor.Editor {
        /// <summary>
        /// The context type this config is closed against. The base editor only shows stages whose
        /// <see cref="PipelineStageAttribute.ContextType"/> matches.
        /// </summary>
        protected abstract Type ContextType { get; }

        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement {
                style = {
                    marginTop = 4
                }
            };

            var title = new Label(target.GetType().Name) {
                style = {
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 2
                }
            };
            root.Add(title);

            var contextLabel = new Label($"Context: {ContextType.Name}") {
                style = {
                    unityFontStyleAndWeight = FontStyle.Italic,
                    color = new Color(0.7f, 0.7f, 0.7f),
                    marginBottom = 4
                }
            };
            root.Add(contextLabel);

            var help = new HelpBox(
                "List order IS execution order — stages run top to bottom. Drag rows to reorder. " +
                "Only stages tagged for this context appear in the picker.",
                HelpBoxMessageType.Info) {
                style = {
                    marginBottom = 8
                }
            };
            root.Add(help);

            var stagesProp = serializedObject.FindProperty("stages");

            if (stagesProp == null) {
                root.Add(new HelpBox(
                    "PipelineConfig is missing its serialized 'stages' field — this should be impossible.",
                    HelpBoxMessageType.Error));

                return root;
            }

            var container = new VisualElement {
                style = {
                    marginBottom = 4
                }
            };
            root.Add(container);

            Refresh();

            var addButton = new Button(() => ShowAddStageMenu(stagesProp, Refresh)) {
                text = "+ Add Stage",
                style = {
                    marginTop = 4
                }
            };

            root.Add(addButton);

            return root;

            void Refresh() {
                serializedObject.Update();
                container.Clear();

                for (var i = 0; i < stagesProp.arraySize; i++)
                    container.Add(BuildStageRow(stagesProp, i, Refresh));
            }
        }

        private VisualElement BuildStageRow(SerializedProperty stagesProp, int index, Action onChanged) {
            var capturedIndex = index;
            var elementProp = stagesProp.GetArrayElementAtIndex(index);

            var row = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 6,
                    paddingTop = 4,
                    paddingBottom = 4,
                    paddingLeft = 6,
                    paddingRight = 6,
                    borderLeftWidth = 3,
                    borderLeftColor = new Color(0.4f, 0.6f, 0.9f),
                    backgroundColor = new Color(0f, 0f, 0f, 0.08f)
                }
            };

            var header = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 4
                }
            };

            var orderLabel = new Label($"{index + 1}.") {
                style = {
                    minWidth = 24,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            header.Add(orderLabel);

            var typeNameLabel = new Label(GetStageDisplayName(elementProp)) {
                style = {
                    flexGrow = 1
                }
            };
            header.Add(typeNameLabel);

            header.Add(EditorListHelpers.IconButton("▲", () => {
                if (EditorListHelpers.MoveUp(stagesProp, capturedIndex))
                    onChanged();
            }));

            header.Add(EditorListHelpers.IconButton("▼", () => {
                if (EditorListHelpers.MoveDown(stagesProp, capturedIndex))
                    onChanged();
            }));

            header.Add(EditorListHelpers.IconButton("✕", () => {
                EditorListHelpers.RemoveAt(stagesProp, capturedIndex);
                onChanged();
            }));

            row.Add(header);

            // Expanded fields of the stage value (if any).
            if (elementProp.managedReferenceValue != null) {
                EditorListHelpers.ForEachVisibleChild(elementProp, child => {
                    var field = new PropertyField(child.Copy());
                    field.Bind(serializedObject);
                    row.Add(field);
                });
            }
            else {
                var empty = new Label("(null — use + Add Stage to assign)") {
                    style = {
                        color = new Color(0.8f, 0.5f, 0.3f),
                        unityFontStyleAndWeight = FontStyle.Italic
                    }
                };
                row.Add(empty);
            }

            return row;
        }

        private void ShowAddStageMenu(SerializedProperty stagesProp, Action onChanged) {
            var menu = new GenericMenu();
            var found = false;

            foreach (var entry in PipelineStageRegistry.GetStagesForContext(ContextType)) {
                found = true;
                var capturedType = entry.Type;

                menu.AddItem(new GUIContent(entry.DisplayName), false, () => {
                    stagesProp.arraySize++;
                    var newElement = stagesProp.GetArrayElementAtIndex(stagesProp.arraySize - 1);
                    newElement.managedReferenceValue = Activator.CreateInstance(capturedType);
                    serializedObject.ApplyModifiedProperties();
                    onChanged();
                });
            }

            if (!found)
                menu.AddDisabledItem(new GUIContent($"No stages registered for {ContextType.Name}"));

            menu.ShowAsContext();
        }

        private string GetStageDisplayName(SerializedProperty elementProp) {
            var managedTypeName = elementProp.managedReferenceFullTypename;

            if (string.IsNullOrEmpty(managedTypeName))
                return "(empty)";

            var typeSimple = managedTypeName;
            var dot = managedTypeName.LastIndexOf('.');

            if (dot >= 0)
                typeSimple = managedTypeName[(dot + 1)..];

            foreach (var entry in PipelineStageRegistry.All) {
                if (entry.Type.Name == typeSimple || entry.Type.FullName == managedTypeName)
                    return entry.DisplayName;
            }

            return typeSimple;
        }
    }

    [CustomEditor(typeof(DamageMitigationPipelineConfig))]
    public sealed class DamageMitigationPipelineConfigEditor : PipelineConfigEditorBase {
        protected override Type ContextType => typeof(DamageMitigationContext);
    }
}