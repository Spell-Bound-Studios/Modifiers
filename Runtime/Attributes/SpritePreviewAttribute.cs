// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Renders an inline preview thumbnail under a <see cref="UnityEngine.Sprite"/> field. Used on
    /// <see cref="StatDefinition.icon"/> so designers see the icon next to the field without selecting the
    /// asset. Implementation: <c>Editor/SpritePreviewDrawer.cs</c>.
    /// </summary>
    public class SpritePreviewAttribute : PropertyAttribute {
        public float Size { get; }

        public SpritePreviewAttribute(float size = 64f) {
            Size = size;
        }
    }
}