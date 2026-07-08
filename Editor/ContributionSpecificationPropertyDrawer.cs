// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Modifiers.Editor {
    [CustomPropertyDrawer(typeof(ContributionSpecification))]
    public sealed class ContributionSpecificationPropertyDrawer : PropertyDrawer {
        private static readonly Dictionary<string, bool> Foldouts = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var stat = property.FindPropertyRelative("stat");
            var type = property.FindPropertyRelative("type");
            var magnitude = property.FindPropertyRelative("magnitude");
            var pairedStat = property.FindPropertyRelative("pairedStat");
            var pairedMagnitude = property.FindPropertyRelative("pairedMagnitude");
            var linkOrdered = property.FindPropertyRelative("linkOrdered");

            var y = position.y;
            y = Draw(position, y, stat, new GUIContent("Stat", "Which stat this line changes."));
            y = Draw(position, y, type, new GUIContent("Modifier Type",
                    "Flat adds a raw amount. Increased / More scale by a percent. Override forces the value."));
            y = Draw(position, y, magnitude, new GUIContent("Amount",
                    "How the amount is decided: a fixed value, a random roll, or scaled from another stat."));

            var open = IsOpen(property, pairedStat);
            var foldRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            open = EditorGUI.Foldout(foldRect, open, new GUIContent("Paired stat (optional)",
                    "Make this line a low-high pair across two stats (a min and a max, rolled together, e.g. 'adds 1 to 10'). Leave collapsed for a single stat."), true);
            Foldouts[property.propertyPath] = open;
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (open) {
                EditorGUI.indentLevel++;
                y = Draw(position, y, pairedStat, new GUIContent("Second Stat",
                        "The high end lands on this stat; leave empty for a normal single-stat line."));
                y = Draw(position, y, pairedMagnitude, new GUIContent("Second Amount", "The high end's amount."));
                Draw(position, y, linkOrdered, new GUIContent("Keep Low <= High",
                        "Re-rolls the high end if it comes out below the low end."));
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("stat"), true) + spacing
                    + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("type"), true) + spacing
                    + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("magnitude"), true) + spacing
                    + EditorGUIUtility.singleLineHeight + spacing;

            if (IsOpen(property, property.FindPropertyRelative("pairedStat"))) {
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("pairedStat"), true) + spacing
                        + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("pairedMagnitude"), true) + spacing
                        + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("linkOrdered"), true) + spacing;
            }

            return height;
        }

        private static bool IsOpen(SerializedProperty property, SerializedProperty pairedStat) {
            return Foldouts.TryGetValue(property.propertyPath, out var open)
                    ? open
                    : pairedStat.objectReferenceValue != null;
        }

        private static float Draw(Rect position, float y, SerializedProperty property, GUIContent label) {
            var height = EditorGUI.GetPropertyHeight(property, label, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), property, label, true);

            return y + height + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
