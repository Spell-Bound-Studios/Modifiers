// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Represents something that provides modifiers (passive nodes, items, buffs, etc.)
    /// </summary>
    public interface IModifierSource {
        /// <summary>
        /// Unique ID for this source. Used for tracking and removal.
        /// </summary>
        int SourceId { get; }

        /// <summary>
        /// Get all modifiers this source provides.
        /// </summary>
        IEnumerable<IModifier> GetModifiers();
    }
}