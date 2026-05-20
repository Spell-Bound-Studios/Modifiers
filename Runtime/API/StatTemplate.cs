// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored pairing of a <see cref="StatDefinition"/> with the base value every instance of the
    /// containing preset spawns with. Lives in the lib so any game using Modifiers can drop these into preset
    /// modules / scriptable objects without re-inventing the shape.
    /// </summary>
    [Serializable, InlineTemplate]
    public struct StatTemplate {
        public StatDefinition definition;
        public float baseValue;
    }
}