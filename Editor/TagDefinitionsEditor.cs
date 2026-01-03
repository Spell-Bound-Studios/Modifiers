// Copyright 2025 Spellbound Studio Inc.

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Spellbound.Stats.Editor {
    [CustomEditor(typeof(TagDefinitions))]
    public class TagDefinitionsEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            
            var tagsProp = serializedObject.FindProperty("tags");
            
            // Check for duplicates
            var names = new List<string>();
            for (var i = 0; i < tagsProp.arraySize; i++) {
                var element = tagsProp.GetArrayElementAtIndex(i);
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
                    $"Duplicate tags found: {string.Join(", ", duplicates)}", 
                    MessageType.Warning
                );
            }
            
            // Track size before drawing
            var oldSize = tagsProp.arraySize;
            
            // Draw default inspector
            DrawDefaultInspector();
            
            // If size increased (+ was clicked), clear the new entry
            if (tagsProp.arraySize > oldSize) {
                var newElement = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
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