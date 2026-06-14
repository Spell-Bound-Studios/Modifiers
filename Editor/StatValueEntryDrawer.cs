// Copyright 2026 Spellbound Studio Inc.

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Editor {
    /// <summary>
    /// Inline row for a <see cref="StatValueEntry"/>: the field's label, a searchable
    /// <see cref="StatDefinition"/> picker, and a float input for the value. The picker is the shared
    /// <see cref="StatDefinitionPicker"/> — same UX as <see cref="StatBaseEntryDrawer"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(StatValueEntry))]
    public sealed class StatValueEntryDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var statProp = property.FindPropertyRelative(nameof(StatValueEntry.stat));
            var valueProp = property.FindPropertyRelative(nameof(StatValueEntry.value));

            var row = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 2
                }
            };

            var label = new Label(property.displayName) {
                tooltip = property.tooltip,
                style = {
                    minWidth = 120,
                    unityTextAlign = TextAnchor.MiddleLeft
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
                var siblings = StatDefinitionPicker.CollectSiblings(property, nameof(StatValueEntry.stat));

                StatDefinitionPicker.Open(pickerButton, siblings, picked => {
                    statProp.objectReferenceValue = picked;
                    statProp.serializedObject.ApplyModifiedProperties();
                    pickerButton.text = StatDefinitionPicker.FormatLabel(picked);
                });
            };

            var valueField = new FloatField {
                value = valueProp.floatValue,
                style = {
                    width = 80
                }
            };

            valueField.RegisterValueChangedCallback(evt => {
                valueProp.floatValue = evt.newValue;
                valueProp.serializedObject.ApplyModifiedProperties();
            });

            valueField.TrackPropertyValue(valueProp, p => valueField.SetValueWithoutNotify(p.floatValue));

            pickerButton.TrackPropertyValue(statProp,
                p => pickerButton.text = StatDefinitionPicker.FormatLabel(p.objectReferenceValue as StatDefinition));

            row.Add(label);
            row.Add(pickerButton);
            row.Add(valueField);

            return row;
        }
    }
}
