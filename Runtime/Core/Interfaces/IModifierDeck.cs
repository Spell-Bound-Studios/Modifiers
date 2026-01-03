// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Implement this if you want to create something that provides a collection of modifiers.
    /// Standardizes "I can give you modifiers".
    /// </summary>
    /// <example>
    /// Items, Passives, Buffs, Characters (aggregating from multiple sources)
    /// </example>
    public interface IModifierDeck {
        /// <summary>
        /// Get all modifiers provided by this deck.
        /// May aggregate from multiple sources or compute dynamically.
        /// </summary>
        public IEnumerable<IModifier> GetModifiers();
    }
}