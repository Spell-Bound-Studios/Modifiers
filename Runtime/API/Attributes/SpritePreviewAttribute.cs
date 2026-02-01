using UnityEngine;

namespace Spellbound.Stats {
    public class SpritePreviewAttribute : PropertyAttribute {
        public float Size { get; }
        
        public SpritePreviewAttribute(float size = 64f) {
            Size = size;
        }
    }
}