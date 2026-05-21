// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// The atomic contract for "something that mutates a target." Two reversible operations
    /// (<see cref="Apply"/> / <see cref="Remove"/>) plus a <see cref="Clone"/> hook used by asset-driven
    /// authoring (see <see cref="ModdedCollection"/>). All gear, buffs, debuffs, talents, conversions,
    /// redirections, and on-event triggers in a PoE-style game are <see cref="IModifier"/> implementations.
    /// </summary>
    /// <example>
    /// NumericModifier (changes stats), OnKillModifier (adds on-kill effects), ConversionModifier (converts damage types)
    /// </example>
    /// <remarks>
    /// Most users inherit <see cref="SbModifier"/> — it bundles a generated <see cref="IHasUniqueId.UniqueId"/>
    /// and helpers for reaching into the target's containers. Implement <see cref="IModifier"/> directly only
    /// when you need to break out of that hierarchy (the documented "20% power user" escape hatch).
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