// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Editor {
    /// <summary>
    /// Custom UI Toolkit drawer for <see cref="BehaviourContainer"/>. Owns the entire rendering so the
    /// inspector reads as a single, polished section instead of "Behaviours → Behaviours" nesting. Each
    /// authored behaviour gets a styled row (accent border, header bar, action buttons) and renders its
    /// contents via <see cref="PropertyField"/>, which delegates to <see cref="SbBehaviourDrawer"/> for the
    /// typed fields + Stats preview. Add button opens a searchable type menu filtered by
    /// <see cref="PickableBehaviourAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(BehaviourContainer))]
    public sealed class BehaviourContainerDrawer : PropertyDrawer {
        // Accent palette — kept here for one-line tweaking; matches PipelineConfigEditor's row styling.
        private static readonly Color AccentBorder = new(0.30f, 0.55f, 0.85f);
        private static readonly Color HeaderBackground = new(0f, 0f, 0f, 0.20f);
        private static readonly Color RowBackground = new(0f, 0f, 0f, 0.08f);
        private static readonly Color EmptyHintColor = new(0.6f, 0.6f, 0.6f);
        private static readonly Color DividerColor = new(1f, 1f, 1f, 0.05f);

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var root = new VisualElement {
                style = {
                    marginTop = 2,
                    marginBottom = 4
                }
            };

            var listProp = EditorListHelpers.FindFirstGenericArrayChild(property);

            if (listProp == null) {
                root.Add(new Label($"BehaviourContainer: no SerializeReference array found at {property.propertyPath}") {
                    style = { color = Color.red }
                });

                return root;
            }

            // Outer card: header + items + add button, in one styled container.
            var card = new VisualElement {
                style = {
                    backgroundColor = RowBackground,
                    borderLeftWidth = 3,
                    borderLeftColor = AccentBorder,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    paddingTop = 6,
                    paddingBottom = 6,
                    paddingLeft = 8,
                    paddingRight = 8
                }
            };

            // Header: title pulled from the OUTER field's display name (so callers control it with field
            // naming), live-updated count badge on the right.
            var header = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 6,
                    paddingBottom = 4,
                    borderBottomWidth = 1,
                    borderBottomColor = DividerColor
                }
            };

            var title = new Label(property.displayName) {
                style = {
                    flexGrow = 1,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 13
                }
            };

            var countBadge = new Label {
                style = {
                    color = EmptyHintColor,
                    fontSize = 11,
                    unityFontStyleAndWeight = FontStyle.Italic
                }
            };

            header.Add(title);
            header.Add(countBadge);
            card.Add(header);

            var itemsContainer = new VisualElement();
            card.Add(itemsContainer);

            Refresh();

            var addButton = new Button(() => ShowAddBehaviourMenu(listProp, Refresh)) {
                text = "+  Add Behaviour",
                style = {
                    marginTop = 6,
                    paddingTop = 4,
                    paddingBottom = 4,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };

            card.Add(addButton);
            root.Add(card);

            return root;

            void Refresh() {
                listProp.serializedObject.Update();
                itemsContainer.Clear();

                countBadge.text = listProp.arraySize == 0
                        ? "empty"
                        : listProp.arraySize == 1
                                ? "1 behaviour"
                                : $"{listProp.arraySize} behaviours";

                if (listProp.arraySize == 0) {
                    itemsContainer.Add(new Label("No behaviours authored. Click \"+ Add Behaviour\" to begin.") {
                        style = {
                            color = EmptyHintColor,
                            unityFontStyleAndWeight = FontStyle.Italic,
                            paddingTop = 8,
                            paddingBottom = 8,
                            unityTextAlign = TextAnchor.MiddleCenter
                        }
                    });

                    return;
                }

                for (var i = 0; i < listProp.arraySize; i++)
                    itemsContainer.Add(BuildBehaviourRow(listProp, i, Refresh));
            }
        }

        // ============================================================================================
        // Per-behaviour row
        // ============================================================================================

        private static VisualElement BuildBehaviourRow(SerializedProperty listProp, int index, Action onChanged) {
            var capturedIndex = index;
            var elementProp = listProp.GetArrayElementAtIndex(index);

            var row = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 6,
                    paddingTop = 4,
                    paddingBottom = 4,
                    paddingLeft = 8,
                    paddingRight = 6,
                    borderLeftWidth = 2,
                    borderLeftColor = AccentBorder,
                    backgroundColor = HeaderBackground,
                    borderTopLeftRadius = 3,
                    borderTopRightRadius = 3,
                    borderBottomLeftRadius = 3,
                    borderBottomRightRadius = 3
                }
            };

            var header = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 4
                }
            };

            var orderLabel = new Label($"#{index + 1}") {
                style = {
                    minWidth = 26,
                    color = EmptyHintColor,
                    fontSize = 11
                }
            };

            var typeName = GetSimpleTypeName(elementProp.managedReferenceFullTypename);

            var typeLabel = new Label(typeName) {
                style = {
                    flexGrow = 1,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 12
                }
            };

            header.Add(orderLabel);
            header.Add(typeLabel);

            header.Add(EditorListHelpers.IconButton("▲", () => {
                if (EditorListHelpers.MoveUp(listProp, capturedIndex))
                    onChanged();
            }));

            header.Add(EditorListHelpers.IconButton("▼", () => {
                if (EditorListHelpers.MoveDown(listProp, capturedIndex))
                    onChanged();
            }));

            header.Add(EditorListHelpers.IconButton("✕", () => {
                EditorListHelpers.RemoveAt(listProp, capturedIndex);
                onChanged();
            }));

            row.Add(header);

            if (elementProp.managedReferenceValue != null) {
                var content = new PropertyField(elementProp);
                content.Bind(listProp.serializedObject);
                row.Add(content);
            }
            else {
                row.Add(new Label("(empty slot — remove or re-pick a type)") {
                    style = {
                        color = new Color(0.8f, 0.5f, 0.3f),
                        unityFontStyleAndWeight = FontStyle.Italic
                    }
                });
            }

            return row;
        }

        // ============================================================================================
        // Type picker (GenericMenu — searchable, scrollable, ESC to cancel)
        // ============================================================================================

        private static void ShowAddBehaviourMenu(SerializedProperty listProp, Action onChanged) {
            var types = EditorListHelpers.GetAssignableTypes(typeof(SbBehaviour), typeof(PickableBehaviourAttribute));

            var menu = new GenericMenu();

            if (types.Count == 0) {
                menu.AddDisabledItem(new GUIContent("No [PickableBehaviour]-tagged types found"));
                menu.ShowAsContext();

                return;
            }

            var present = new HashSet<Type>();

            for (var i = 0; i < listProp.arraySize; i++) {
                if (listProp.GetArrayElementAtIndex(i).managedReferenceValue is { } existing)
                    present.Add(existing.GetType());
            }

            foreach (var type in types) {
                if (present.Contains(type)) {
                    menu.AddDisabledItem(new GUIContent($"{type.Name}  ✓ added"));

                    continue;
                }

                var captured = type;

                menu.AddItem(new GUIContent(captured.Name), false, () => {
                    listProp.arraySize++;
                    var newElement = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
                    newElement.managedReferenceValue = Activator.CreateInstance(captured);
                    listProp.serializedObject.ApplyModifiedProperties();
                    onChanged();
                });
            }

            menu.ShowAsContext();
        }

        // ============================================================================================
        // Helpers
        // ============================================================================================

        private static string GetSimpleTypeName(string managedReferenceFullTypename) {
            if (string.IsNullOrEmpty(managedReferenceFullTypename))
                return "(empty)";

            // Format is "AssemblyName TypeFullName"; we want the last segment of TypeFullName.
            var parts = managedReferenceFullTypename.Split(' ');
            var typeFullName = parts.Length >= 2 ? parts[1] : managedReferenceFullTypename;

            var dot = typeFullName.LastIndexOf('.');

            return dot >= 0 ? typeFullName[(dot + 1)..] : typeFullName;
        }
    }
}