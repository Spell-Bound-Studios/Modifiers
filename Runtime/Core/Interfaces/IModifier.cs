// Copyright 2025 Spellbound Studio Inc.

#nullable enable
using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Implement this if you want to create a custom modifier type.
    /// Modifiers can change stats, add behaviours, or modify entity capabilities.
    /// </summary>
    /// <example>
    /// NumericModifier (changes stats), OnKillModifier (adds on-kill effects), ConversionModifier (converts damage types)
    /// </example>
    /// <remarks>
    /// This is a primitive component of this entire architecture. It's very important to remember that IModifier
    /// represents a single modification... and the ModifierDefinition represents a collection of modifications.
    /// </remarks>
    public interface IModifier {
        /// <summary>
        /// Unique identifier for this modifier instance.
        /// </summary>
        /// <remarks>
        /// The intention behind this ID is to differentiate between the two modifiers on something. This tells us
        /// which mod belongs to which IModifier. Imagine you allowed your game to have hybrid modifier definitions that
        /// grant 7 strength and 7 armor and then another modifier definition of 10 strength... then you go to remove one
        /// how do you know which one to remove?
        /// </remarks>
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
        void Apply(IModifiable target);

        /// <summary>
        /// Remove this modifier from a target entity.
        /// Called when the modifier source is removed.
        /// </summary>
        void Remove(IModifiable target);
    }
}