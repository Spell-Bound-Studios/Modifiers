// Copyright 2026 Spellbound Studio Inc.

using UnityEditor;
using UnityEngine;

namespace Spellbound.Modifiers.Editor {
    /// <summary>
    /// Property drawer powering <see cref="SpritePreviewAttribute"/>. Renders the default sprite picker on
    /// line one and a square preview thumbnail below it (sized from <see cref="SpritePreviewAttribute.Size"/>)
    /// when a sprite is assigned. IMGUI-only — drawn fields don't currently appear inside UI Toolkit
    /// inspectors.
    /// </summary>
    [CustomPropertyDrawer(typeof(SpritePreviewAttribute))]
    public class SpritePreviewDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var attr = (SpritePreviewAttribute)attribute;
            var size = attr.Size;

            // Draw the default sprite field
            var fieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(fieldRect, property, label);

            // Draw sprite preview if assigned
            var sprite = property.objectReferenceValue as Sprite;

            if (sprite == null)
                return;

            var previewRect = new Rect(
                position.x + EditorGUIUtility.labelWidth,
                position.y + EditorGUIUtility.singleLineHeight + 4,
                size,
                size
            );

            EditorGUI.DrawPreviewTexture(previewRect, sprite.texture, null, ScaleMode.ScaleToFit);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var attr = (SpritePreviewAttribute)attribute;
            var baseHeight = EditorGUIUtility.singleLineHeight;

            var sprite = property.objectReferenceValue as Sprite;

            if (sprite != null)
                return baseHeight + attr.Size + 8;

            return baseHeight;
        }
    }
}