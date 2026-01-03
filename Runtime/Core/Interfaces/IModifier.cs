// Copyright 2025 Spellbound Studio Inc.

#nullable enable
using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// The atomic primitive for a modifier.
    /// Implement this if you want to create a custom modifier.
    /// Defines what it means to be a modification that can be applied/removed
    /// </summary>
    /// <example>
    /// NumericModifier (changes stats), OnKillModifier (adds on-kill effects), ConversionModifier (converts damage types)
    /// </example>
    /// <remarks>
    /// This is a primitive component of this entire architecture. It's very important to remember that IModifier
    /// represents a single modification.
    /// </remarks>
    public interface IModifier {
        /// <summary>
        /// Unique identifier for this modifier instance.
        /// Used to track individual modifiers for stacking rules and granular removal.
        /// </summary>
        int ModifierId { get; }

        /// <summary>
        /// Tags that determine what this modifier can affect.
        /// Empty/Null RequiredTags means applies to everything.
        /// </summary>
        /// <example>
        /// [Fire, Spell] = applies to anything with Fire or Spell tags
        /// Null = applies unconditionally.
        /// </example>
        HashSet<int>? RequiredTags { get; }

        /// <summary>
        /// Apply this modifier to a target entity.
        /// Should check if target's tags match RequiredTags.
        /// </summary>
        void Apply(ICanBeModified target);

        /// <summary>
        /// Remove this modifier from a target entity.
        /// Called when the modifier source is removed.
        /// </summary>
        void Remove(ICanBeModified target);
    }
}