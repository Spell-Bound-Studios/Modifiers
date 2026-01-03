// Copyright 2025 Spellbound Studio Inc.

#if UNITY_EDITOR
using Spellbound.Stats.Attributes;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Stats.Editor {
    [CustomPropertyDrawer(typeof(StatIdAttribute))]
    public class StatIdAttributeDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.propertyType != SerializedPropertyType.Integer) {
                EditorGUI.LabelField(position, label.text, "[StatId] only works on int fields");
                return;
            }

            DefinitionDropdownUtility.DrawStatDropdown(position, property, label);
        }
    }
}
#endif