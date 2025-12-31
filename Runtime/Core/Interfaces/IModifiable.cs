// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Implement this if you want something to be able to receive modifiers.
    /// Tags determine which modifiers are applicable to this entity.
    /// </summary>
    /// <example>
    /// A players stats can be modified and should therefore implement the IModifiable. The interface will be leveraged
    /// to check if the target has matching tags before it attempts to modify them.
    /// </example>
    public interface IModifiable {
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