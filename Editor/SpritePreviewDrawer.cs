using UnityEditor;
using UnityEngine;

namespace Spellbound.Stats.Editor {
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