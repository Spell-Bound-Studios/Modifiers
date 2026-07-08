// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Modifiers.Editor {
    [CustomPropertyDrawer(typeof(Magnitude), true)]
    [CustomPropertyDrawer(typeof(Condition), true)]
    public sealed class SerializeReferencePicker : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var types = AssignableTypes(property);
            var current = property.managedReferenceValue?.GetType();
            var currentIndex = current == null ? 0 : Array.IndexOf(types, current) + 1;

            var options = new GUIContent[types.Length + 1];
            options[0] = new GUIContent("None");

            for (var i = 0; i < types.Length; i++)
                options[i + 1] = new GUIContent(FriendlyName(types[i]));

            var selected = EditorGUI.Popup(line, label, currentIndex, options);

            if (selected != currentIndex) {
                property.managedReferenceValue = selected == 0 ? null : Activator.CreateInstance(types[selected - 1]);
                property.serializedObject.ApplyModifiedProperties();
                EditorGUI.EndProperty();

                return;
            }

            if (property.managedReferenceValue != null) {
                EditorGUI.indentLevel++;
                var y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                var child = property.Copy();
                var end = child.GetEndProperty();

                if (child.NextVisible(true)) {
                    do {
                        if (SerializedProperty.EqualContents(child, end))
                            break;

                        var height = EditorGUI.GetPropertyHeight(child, true);
                        var content = new GUIContent(child.displayName, child.tooltip);
                        EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), child, content, true);
                        y += height + EditorGUIUtility.standardVerticalSpacing;
                    } while (child.NextVisible(false));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var height = EditorGUIUtility.singleLineHeight;

            if (property.managedReferenceValue == null)
                return height;

            var child = property.Copy();
            var end = child.GetEndProperty();

            if (child.NextVisible(true)) {
                do {
                    if (SerializedProperty.EqualContents(child, end))
                        break;

                    height += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
                } while (child.NextVisible(false));
            }

            return height;
        }

        private static string FriendlyName(Type type) {
            var label = type.GetCustomAttribute<SerializeReferenceLabelAttribute>();

            return label != null ? label.Label : ObjectNames.NicifyVariableName(type.Name);
        }

        private Type[] AssignableTypes(SerializedProperty property) {
            var fieldType = FieldTypeOf(property);

            return TypeCache.GetTypesDerivedFrom(fieldType)
                    .Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition)
                    .OrderBy(FriendlyName)
                    .ToArray();
        }

        private Type FieldTypeOf(SerializedProperty property) {
            return TypeFromTypename(property.managedReferenceFieldTypename) ?? fieldInfo?.FieldType ?? typeof(object);
        }

        private static Type TypeFromTypename(string typename) {
            if (string.IsNullOrEmpty(typename))
                return null;

            var split = typename.IndexOf(' ');

            if (split < 0)
                return null;

            var assembly = typename[..split];
            var fullName = typename[(split + 1)..];

            return Type.GetType($"{fullName}, {assembly}");
        }
    }
}
