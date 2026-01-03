// Copyright 2025 Spellbound Studio Inc.

#if UNITY_EDITOR
using System;
using System.Reflection;
using Spellbound.Stats.Attributes;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Spellbound.Stats.Editor {
    [CustomPropertyDrawer(typeof(ModifierModule))]
    public class ModifierModuleDrawer : PropertyDrawer {
        private SerializedProperty _currentProperty;
        private ReorderableList _modifiersList;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var fixedModsProp = property.FindPropertyRelative("fixedMods");

            if (fixedModsProp == null)
                return EditorGUIUtility.singleLineHeight;

            var height = EditorGUIUtility.singleLineHeight + 2;

            if (!property.isExpanded)
                return height;

            GetOrCreateList(property);

            if (_modifiersList != null)
                height += _modifiersList.GetHeight() + 2;

            height += EditorGUIUtility.singleLineHeight + 2;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            property.FindPropertyRelative("fixedMods");
            var numberOfModsProp = property.FindPropertyRelative("numberOfMods");

            var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (property.isExpanded) {
                EditorGUI.indentLevel++;

                position.y += EditorGUIUtility.singleLineHeight + 2;

                GetOrCreateList(property);

                if (_modifiersList != null) {
                    var listRect = new Rect(position.x, position.y, position.width, _modifiersList.GetHeight());
                    _modifiersList.DoList(listRect);
                    position.y += _modifiersList.GetHeight() + 2;
                }

                var numberOfModsRect =
                        new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(numberOfModsRect, numberOfModsProp);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        #region Property Drawing with Attribute Support

        private static void DrawPropertyWithAttributeSupport(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (!property.isExpanded) {
                EditorGUI.EndProperty();

                return;
            }

            position.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.indentLevel++;

            var iterator = property.Copy();
            var endProperty = property.GetEndProperty();

            var enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty)) {
                enterChildren = false;

                var fieldRect = new Rect(position.x, position.y, position.width,
                    EditorGUI.GetPropertyHeight(iterator, true));

                if (HasStatIdAttribute(property, iterator.name))
                    DrawStatIdField(fieldRect, iterator);
                else if (HasTagIdAttribute(property, iterator.name))
                    DrawTagIdField(fieldRect, iterator);
                else
                    EditorGUI.PropertyField(fieldRect, iterator, true);

                position.y += EditorGUI.GetPropertyHeight(iterator, true) + 2;
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        #endregion

        #region ReorderableList Setup

        private void GetOrCreateList(SerializedProperty property) {
            if (_modifiersList != null && _currentProperty == property)
                return;

            _currentProperty = property;
            var fixedModsProp = property.FindPropertyRelative("fixedMods");

            _modifiersList = new ReorderableList(
                property.serializedObject,
                fixedModsProp,
                true, true, true, true
            ) {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Fixed Modifiers"),

                elementHeightCallback = index => {
                    var element = fixedModsProp.GetArrayElementAtIndex(index);

                    return EditorGUI.GetPropertyHeight(element, true) + 4;
                },

                drawElementCallback = (rect, index, _, _) => {
                    var element = fixedModsProp.GetArrayElementAtIndex(index);

                    var typeName = GetTypeName(element);

                    rect.x += 10;
                    rect.width -= 10;
                    rect.y += 2;
                    rect.height -= 4;

                    DrawPropertyWithAttributeSupport(rect, element, new GUIContent(typeName));
                },

                onAddDropdownCallback = (buttonRect, _) => {
                    var menu = new GenericMenu();

                    foreach (var type in TypeCache.GetTypesDerivedFrom<IModifier>()) {
                        if (type.IsAbstract || type.IsInterface)
                            continue;

                        var typeName = type.Name;

                        menu.AddItem(new GUIContent(typeName), false, () => {
                            fixedModsProp.arraySize++;
                            var newElement = fixedModsProp.GetArrayElementAtIndex(fixedModsProp.arraySize - 1);
                            newElement.managedReferenceValue = Activator.CreateInstance(type);
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }

                    menu.DropDown(buttonRect);
                }
            };
        }

        private string GetTypeName(SerializedProperty property) {
            if (property.managedReferenceValue == null)
                return "None";

            var fullType = property.managedReferenceFullTypename;
            var lastSpace = fullType.LastIndexOf(' ');

            if (lastSpace >= 0)
                fullType = fullType[(lastSpace + 1)..];

            var lastDot = fullType.LastIndexOf('.');

            return lastDot >= 0 ? fullType[(lastDot + 1)..] : fullType;
        }

        #endregion

        #region Attribute Detection

        private static bool HasStatIdAttribute(SerializedProperty parentProperty, string fieldName) {
            if (parentProperty.managedReferenceValue == null)
                return false;

            var targetType = parentProperty.managedReferenceValue.GetType();

            var field = targetType.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            return field != null && Attribute.IsDefined(field, typeof(StatIdAttribute));
        }

        private static bool HasTagIdAttribute(SerializedProperty parentProperty, string fieldName) {
            if (parentProperty.managedReferenceValue == null)
                return false;

            var targetType = parentProperty.managedReferenceValue.GetType();

            var field = targetType.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            return field != null && Attribute.IsDefined(field, typeof(TagIdAttribute));
        }

        #endregion

        #region Drawing with Utility

        private static void DrawStatIdField(Rect position, SerializedProperty property) =>
                DefinitionDropdownUtility.DrawStatDropdown(position, property, new GUIContent(property.displayName));

        private static void DrawTagIdField(Rect position, SerializedProperty property) =>
                DefinitionDropdownUtility.DrawTagDropdown(position, property, new GUIContent(property.displayName));

        #endregion
    }
}
#endif