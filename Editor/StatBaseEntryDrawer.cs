// Copyright 2026 Spellbound Studio Inc.

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Editor {
    /// <summary>
    /// Inline row for a <see cref="StatBaseEntry"/>: a searchable <see cref="StatDefinition"/> picker on the
    /// left and a float input for the base value on the right. The picker is the shared
    /// <see cref="StatDefinitionPicker"/> — same UX as <see cref="ResourceBaseEntryDrawer"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(StatBaseEntry))]
    public sealed class StatBaseEntryDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var statProp = property.FindPropertyRelative(nameof(StatBaseEntry.stat));
            var baseValueProp = property.FindPropertyRelative(nameof(StatBaseEntry.baseValue));

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
                var siblings = StatDefinitionPicker.CollectSiblings(property, nameof(StatBaseEntry.stat));

                StatDefinitionPicker.Open(pickerButton.worldBound, siblings, picked => {
                    statProp.objectReferenceValue = picked;
                    statProp.serializedObject.ApplyModifiedProperties();
                    pickerButton.text = StatDefinitionPicker.FormatLabel(picked);
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
                p => pickerButton.text = StatDefinitionPicker.FormatLabel(p.objectReferenceValue as StatDefinition));

            row.Add(pickerButton);
            row.Add(valueField);

            return row;
        }
    }
}
