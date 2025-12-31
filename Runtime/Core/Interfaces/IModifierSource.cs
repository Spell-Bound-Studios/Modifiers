// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Implement this if you want to create something that provides modifiers.
    /// Sources aggregate and distribute modifiers to targets with stats (like characters or skills).
    /// </summary>
    /// <example>
    /// Items, Passive Nodes, Buffs, Debuffs, Auras
    /// </example>
    public interface IModifierSource {
        /// <summary>
        /// Unique identifier for this modifier source.
        /// </summary>
        /// <example>
        /// Item's InstanceID, Passive node's ID, Buff's ID
        /// </example>
        int SourceId { get; }

        /// <summary>
        /// Get all modifiers this source provides.
        /// Called when a modifier is applied or removed from a target.
        /// </summary>
        IEnumerable<IModifier> GetModifiers();
    }
}