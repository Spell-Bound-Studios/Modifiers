// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// The atomic primitive for a modifier.
    /// Implement this if you want to create a custom modifier.
    /// Defines what it means to be a modification that can be applied/removed
    /// </summary>
    /// <example>
    /// NumericModifier (changes stats), OnKillModifier (adds on-kill effects), ConversionModifier (converts damage types)
    /// </example>
    /// <remarks>
    /// This is a primitive component of the entire architecture. It's very important to remember that IModifier
    /// represents a single modification in its purest form.
    /// </remarks>
    public interface IModifier {
        /// <summary>
        /// Apply this modifier to a target entity.
        /// </summary>
        void Apply(ICanBeModified target);

        /// <summary>
        /// Remove this modifier from a target entity.
        /// Called when the modifier source is removed.
        /// </summary>
        void Remove(ICanBeModified target);

        IModifier Clone();
    }
}