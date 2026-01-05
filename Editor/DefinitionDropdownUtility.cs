// Copyright 2025 Spellbound Studio Inc.

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Stats.Editor {
    /// <summary>
    /// Shared utility for drawing stat definition dropdowns.
    /// </summary>
    public static class DefinitionDropdownUtility {
        private static StatDefinitions _cachedStatDatabase;
        private static bool _statDatabaseSearched;

        public static void DrawStatDropdown(Rect position, SerializedProperty property, GUIContent label) {
            var database = GetStatDatabase();

            if (database == null) {
                EditorGUI.PropertyField(position, property, label);

                return;
            }

            var names = database.GetAllStatNames().ToArray();

            if (names.Length == 0) {
                EditorGUI.LabelField(position, label.text, "No stats defined");

                return;
            }

            var currentId = property.intValue;
            var currentIndex = -1;

            for (var i = 0; i < names.Length; i++) {
                if (database.GetStatId(names[i]) != currentId)
                    continue;

                currentIndex = i;

                break;
            }

            var newIndex = EditorGUI.Popup(position, label.text, currentIndex, names);

            if (newIndex >= 0 && newIndex != currentIndex)
                property.intValue = database.GetStatId(names[newIndex]);
        }

        public static StatDefinitions GetStatDatabase() {
            if (_statDatabaseSearched)
                return _cachedStatDatabase;

            _statDatabaseSearched = true;

            var guids = AssetDatabase.FindAssets("t:StatDefinitions");

            if (guids is not { Length: > 0 })
                return _cachedStatDatabase;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _cachedStatDatabase = AssetDatabase.LoadAssetAtPath<StatDefinitions>(path);

            return _cachedStatDatabase;
        }
    }
}
#endif