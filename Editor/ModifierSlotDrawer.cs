// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Spellbound.Stats.Editor {
    /// <summary>
    /// Custom property drawer for ModifierSlot.
    /// Displays a dropdown of available ModifierDefinitions and shows value previews.
    /// </summary>
    [CustomPropertyDrawer(typeof(ModifierSlot))]
    public class ModifierSlotDrawer : PropertyDrawer {
        private const float Padding = 4f;
        private static readonly float LineHeight = EditorGUIUtility.singleLineHeight;
        private static readonly float Spacing = EditorGUIUtility.standardVerticalSpacing;

        private static string[] _cachedModifierNames;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
                LineHeight * 3 + Spacing * 3 + Padding * 2;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var backgroundRect = new Rect(position.x, position.y, position.width, GetPropertyHeight(property, label));
            EditorGUI.DrawRect(backgroundRect, new Color(0f, 0f, 0f, 0.1f));

            position.x += Padding;
            position.y += Padding;
            position.width -= Padding * 2;

            var modifierNameProp = property.FindPropertyRelative("modifierName");
            var rollTypeProp = property.FindPropertyRelative("rollType");

            // Line 1: Modifier dropdown
            var modifierRect = new Rect(position.x, position.y, position.width, LineHeight);
            var selectedModifier = DrawModifierDropdown(modifierRect, modifierNameProp);

            // Line 2: Roll type
            position.y += LineHeight + Spacing;
            var rollTypeRect = new Rect(position.x, position.y, position.width, LineHeight);
            EditorGUI.PropertyField(rollTypeRect, rollTypeProp, new GUIContent("Roll Type"));

            // Line 3: Value preview - pass the roll type
            position.y += LineHeight + Spacing;
            var previewRect = new Rect(position.x, position.y, position.width, LineHeight);
            DrawValuePreview(previewRect, selectedModifier, (ModifierRollType)rollTypeProp.enumValueIndex);

            EditorGUI.EndProperty();
        }

        private string DrawModifierDropdown(Rect rect, SerializedProperty modifierNameProp) {
            var allModifiers = GetAllModifierTypes();

            if (allModifiers.Length == 0) {
                EditorGUI.LabelField(rect, "Modifier", "No ModifierDefinition classes found");
                return "";
            }

            var currentName = modifierNameProp.stringValue;
            var currentIndex = Array.IndexOf(allModifiers, currentName);
            if (currentIndex == -1) currentIndex = 0;

            EditorGUI.BeginChangeCheck();
            var newIndex = EditorGUI.Popup(rect, "Modifier", currentIndex, allModifiers);

            if (!EditorGUI.EndChangeCheck()) 
                return allModifiers[newIndex];

            modifierNameProp.stringValue = allModifiers[newIndex];
            modifierNameProp.serializedObject.ApplyModifiedProperties();

            return allModifiers[newIndex];
        }

        private static void DrawValuePreview(Rect rect, string modifierName, ModifierRollType rollType) {
            if (string.IsNullOrEmpty(modifierName)) {
                EditorGUI.LabelField(rect, "Values", "Select a modifier");
                return;
            }

            var definition = CreateModifierDefinitionByName(modifierName);
            
            if (definition == null) {
                EditorGUI.LabelField(rect, "Values", "Type not found");
                return;
            }

            // Call GetValuePreview with the roll type
            var preview = definition.GetValuePreview(rollType);
            EditorGUI.LabelField(rect, "Values", preview);
        }

        /// <summary>
        /// Find all ModifierDefinition subclasses via reflection.
        /// Cached for performance.
        /// </summary>
        private static string[] GetAllModifierTypes() {
            if (_cachedModifierNames != null)
                return _cachedModifierNames;

            var modifierTypes = new List<string>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies) {
                try {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ModifierDefinition)));
                    
                    foreach (var type in types) {
                        var instance = (ModifierDefinition)Activator.CreateInstance(type);
                        modifierTypes.Add(instance.DisplayName);
                    }
                } catch {
                    // Skip assemblies that can't be loaded
                }
            }

            _cachedModifierNames = modifierTypes.OrderBy(n => n).ToArray();
            return _cachedModifierNames;
        }

        /// <summary>
        /// Create a ModifierDefinition instance by its DisplayName.
        /// </summary>
        private static ModifierDefinition CreateModifierDefinitionByName(string displayName) {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies) {
                try {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ModifierDefinition)));
                    
                    foreach (var type in types) {
                        var instance = (ModifierDefinition)Activator.CreateInstance(type);
                        if (instance.DisplayName == displayName)
                            return instance;
                    }
                } catch {
                    // Skip
                }
            }

            return null;
        }
        
        /// <summary>
        /// Clear cache when scripts recompile.
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded() {
            _cachedModifierNames = null;
        }
    }
}