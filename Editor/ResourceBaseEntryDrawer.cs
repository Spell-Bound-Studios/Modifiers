// Copyright 2026 Spellbound Studio Inc.

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Editor {
    /// <summary>
    /// Inline row for a <see cref="ResourceBaseEntry"/>: searchable <see cref="StatDefinition"/> picker on
    /// the left, then float inputs for <c>base</c> and <c>min</c>. Picker is the shared
    /// <see cref="StatDefinitionPicker"/> — same UX as <see cref="StatBaseEntryDrawer"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(ResourceBaseEntry))]
    public sealed class ResourceBaseEntryDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var statProp = property.FindPropertyRelative(nameof(ResourceBaseEntry.stat));
            var baseValueProp = property.FindPropertyRelative(nameof(ResourceBaseEntry.baseValue));
            var minProp = property.FindPropertyRelative(nameof(ResourceBaseEntry.min));

            var row = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 2
                }
            };

            var pickerButton = new Button {
                text = StatDefinitionPicker.FormatLabel(statProp.objectReferenceValue as StatDefinition),
                style = {
                    flexGrow = 1,
                    marginRight = 4,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    paddingLeft = 8
                }
            };

            pickerButton.clicked += () => {
                var siblings = StatDefinitionPicker.CollectSiblings(property, nameof(ResourceBaseEntry.stat));

                StatDefinitionPicker.Open(pickerButton, siblings, picked => {
                    statProp.objectReferenceValue = picked;
                    statProp.serializedObject.ApplyModifiedProperties();
                    pickerButton.text = StatDefinitionPicker.FormatLabel(picked);
                });
            };

            var baseField = new FloatField("base") {
                value = baseValueProp.floatValue,
                style = {
                    width = 120,
                    marginRight = 4
                }
            };

            baseField.labelElement.style.minWidth = 32;

            baseField.RegisterValueChangedCallback(evt => {
                baseValueProp.floatValue = evt.newValue;
                baseValueProp.serializedObject.ApplyModifiedProperties();
            });

            var minField = new FloatField("min") {
                value = minProp.floatValue,
                style = {
                    width = 110
                }
            };

            minField.labelElement.style.minWidth = 28;

            minField.RegisterValueChangedCallback(evt => {
                minProp.floatValue = evt.newValue;
                minProp.serializedObject.ApplyModifiedProperties();
            });

            baseField.TrackPropertyValue(baseValueProp, p => baseField.SetValueWithoutNotify(p.floatValue));
            minField.TrackPropertyValue(minProp, p => minField.SetValueWithoutNotify(p.floatValue));

            pickerButton.TrackPropertyValue(statProp,
                p => pickerButton.text = StatDefinitionPicker.FormatLabel(p.objectReferenceValue as StatDefinition));

            row.Add(pickerButton);
            row.Add(baseField);
            row.Add(minField);

            return row;
        }
    }
}