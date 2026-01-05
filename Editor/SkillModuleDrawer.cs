// Copyright 2025 Spellbound Studio Inc.

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Spellbound.Stats.Editor {
    [CustomPropertyDrawer(typeof(SkillModule))]
    public class SkillModuleDrawer : PropertyDrawer {
        private ReorderableList _behavioursList;
        private SerializedProperty _currentProperty;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var behavioursProperty = property.FindPropertyRelative("behaviours");

            if (behavioursProperty == null)
                return EditorGUIUtility.singleLineHeight;

            var height = EditorGUIUtility.singleLineHeight + 2;

            if (!property.isExpanded)
                return height;

            // Height for skillName
            height += EditorGUIUtility.singleLineHeight + 2;

            // Height for behaviours list
            GetOrCreateList(property);

            if (_behavioursList != null)
                height += _behavioursList.GetHeight() + 2;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (property.isExpanded) {
                EditorGUI.indentLevel++;
                position.y += EditorGUIUtility.singleLineHeight + 2;

                // Draw skillName
                var skillNameProperty = property.FindPropertyRelative("skillName");
                var nameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(nameRect, skillNameProperty);
                position.y += EditorGUIUtility.singleLineHeight + 2;

                // Draw behaviours list
                GetOrCreateList(property);

                if (_behavioursList != null) {
                    var listRect = new Rect(position.x, position.y, position.width, _behavioursList.GetHeight());
                    _behavioursList.DoList(listRect);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private void GetOrCreateList(SerializedProperty property) {
            if (_behavioursList != null && _currentProperty == property)
                return;

            _currentProperty = property;
            var behavioursProperty = property.FindPropertyRelative("behaviours");

            _behavioursList = new ReorderableList(
                property.serializedObject,
                behavioursProperty,
                true, true, true, true
            ) {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Behaviours"),

                elementHeightCallback = index => {
                    var element = behavioursProperty.GetArrayElementAtIndex(index);

                    return EditorGUI.GetPropertyHeight(element, true) + 4;
                },

                drawElementCallback = (rect, index, _, _) => {
                    var element = behavioursProperty.GetArrayElementAtIndex(index);

                    var typeName = GetTypeName(element);

                    rect.x += 10;
                    rect.width -= 10;
                    rect.y += 2;
                    rect.height -= 4;

                    EditorGUI.PropertyField(rect, element, new GUIContent(typeName), true);
                },

                onAddDropdownCallback = (buttonRect, _) => {
                    var menu = new GenericMenu();

                    foreach (var type in TypeCache.GetTypesDerivedFrom<IBehaviour>()) {
                        if (type.IsAbstract || type.IsInterface)
                            continue;

                        var typeName = type.Name;

                        menu.AddItem(new GUIContent(typeName), false, () => {
                            behavioursProperty.arraySize++;

                            var newElement =
                                    behavioursProperty.GetArrayElementAtIndex(behavioursProperty.arraySize - 1);
                            newElement.managedReferenceValue = Activator.CreateInstance(type);
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }

                    menu.DropDown(buttonRect);
                }
            };
        }

        private static string GetTypeName(SerializedProperty property) {
            if (property.managedReferenceValue == null)
                return "None";

            var fullType = property.managedReferenceFullTypename;
            var lastSpace = fullType.LastIndexOf(' ');

            if (lastSpace >= 0)
                fullType = fullType[(lastSpace + 1)..];

            var lastDot = fullType.LastIndexOf('.');

            return lastDot >= 0 ? fullType[(lastDot + 1)..] : fullType;
        }
    }
}
#endif