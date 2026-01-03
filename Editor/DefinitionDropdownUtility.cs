// Copyright 2025 Spellbound Studio Inc.

#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Stats.Editor {
    /// <summary>
    /// Shared utility for drawing definition dropdowns (stats, tags, etc.)
    /// </summary>
    public static class DefinitionDropdownUtility {
        private static StatDefinitions _cachedStatDatabase;
        private static TagDefinitions _cachedTagDatabase;
        private static bool _statDatabaseSearched;
        private static bool _tagDatabaseSearched;

        public static void DrawStatDropdown(Rect position, SerializedProperty property, GUIContent label) {
            var database = GetStatDatabase();

            DrawDropdown(position, property, label, database,
                db => db.GetAllStatNames().ToArray(),
                (db, name) => db.GetStatId(name),
                "No stats defined");
        }

        public static void DrawTagDropdown(Rect position, SerializedProperty property, GUIContent label) {
            var database = GetTagDatabase();

            DrawDropdown(position, property, label, database,
                db => db.GetAllTagNames().ToArray(),
                (db, name) => db.GetTagId(name),
                "No tags defined");
        }

        private static void DrawDropdown<T>(
            Rect position,
            SerializedProperty property,
            GUIContent label,
            T database,
            Func<T, string[]> getNamesFunc,
            Func<T, string, int> getIdFunc,
            string emptyMessage) where T : class {
            if (database == null) {
                EditorGUI.PropertyField(position, property, label);

                return;
            }

            var names = getNamesFunc(database);

            if (names.Length == 0) {
                EditorGUI.LabelField(position, label.text, emptyMessage);

                return;
            }

            var currentId = property.intValue;
            var currentIndex = -1;

            for (var i = 0; i < names.Length; i++) {
                if (getIdFunc(database, names[i]) != currentId) 
                    continue;

                currentIndex = i;

                break;
            }

            var newIndex = EditorGUI.Popup(position, label.text, currentIndex, names);

            if (newIndex >= 0 && newIndex != currentIndex)
                property.intValue = getIdFunc(database, names[newIndex]);
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

        public static TagDefinitions GetTagDatabase() {
            if (_tagDatabaseSearched)
                return _cachedTagDatabase;

            _tagDatabaseSearched = true;

            var guids = AssetDatabase.FindAssets("t:TagDefinitions");

            if (guids is not { Length: > 0 }) 
                return _cachedTagDatabase;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _cachedTagDatabase = AssetDatabase.LoadAssetAtPath<TagDefinitions>(path);

            return _cachedTagDatabase;
        }
    }
}
#endif