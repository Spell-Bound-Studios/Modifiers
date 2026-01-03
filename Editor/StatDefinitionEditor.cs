// Copyright 2025 Spellbound Studio Inc.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Spellbound.Stats.Editor {
    [CustomEditor(typeof(StatDefinitions))]
    public class StatDefinitionsEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();

            var statsProp = serializedObject.FindProperty("stats");

            // Check for duplicates
            var names = new List<string>();

            for (var i = 0; i < statsProp.arraySize; i++) {
                var element = statsProp.GetArrayElementAtIndex(i);
                var nameProp = element.FindPropertyRelative("name");

                if (!string.IsNullOrEmpty(nameProp.stringValue))
                    names.Add(nameProp.stringValue);
            }

            var duplicates = names.GroupBy(x => x)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

            // Show warning box if duplicates exist
            if (duplicates.Count > 0) {
                EditorGUILayout.HelpBox(
                    $"Duplicate stats found: {string.Join(", ", duplicates)}",
                    MessageType.Warning
                );
            }

            // Track size before drawing
            var oldSize = statsProp.arraySize;

            // Draw default inspector
            DrawDefaultInspector();

            // If size increased (+ was clicked), clear the new entry
            if (statsProp.arraySize > oldSize) {
                var newElement = statsProp.GetArrayElementAtIndex(statsProp.arraySize - 1);
                var nameProp = newElement.FindPropertyRelative("name");
                var idProp = newElement.FindPropertyRelative("id");

                nameProp.stringValue = "";
                idProp.intValue = 0;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif