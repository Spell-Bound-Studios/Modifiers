// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Implement this if you want something to be able to receive modifiers - it simply marks it as being modifiable.
    /// Tags determine which modifiers are applicable to this entity.
    /// </summary>
    /// <example>
    /// Skills, Characters, Items - anything that modifiers can affect.
    /// </example>
    public interface ICanBeModified {
        /// <summary>
        /// Tags that determine which modifiers affect this entity.
        /// Stored as integer IDs registered in TagRegistry.
        /// </summary>
        /// <example>
        /// "Fire", "Cold", "Spell", "Attack", "Projectile", "Physical"
        /// </example>
        HashSet<int> Tags { get; }
    }
}