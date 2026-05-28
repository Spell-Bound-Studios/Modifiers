// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Abstract ScriptableObject base for designer-authored, registry-indexed modifier definitions.
    /// Every game-side "affix" or "named effect" (a +X life roll, an Iron Will buff, a fire-damage
    /// implicit, a talent's bonus) is one of these assets. Items, talents, drops, and tooltips all
    /// reference NamedModifier assets — single source of truth per affix.
    /// </summary>
    /// <remarks>
    /// <para><b>Identity.</b> Every NamedModifier has a stable string <see cref="Key"/>
    /// (snake_case convention) and a player-facing <see cref="DisplayName"/>. The
    /// <see cref="NamedModifierRegistry"/> indexes assets by both Key (designer-facing) and by
    /// a deterministic uint hash of Key (for compact packing into save files and network frames).
    /// Same string → same hash → same id, every run, every machine.</para>
    /// <para><b>Instantiation.</b> <see cref="Instantiate"/> produces an
    /// <see cref="SbModifier"/> instance configured with the given value (for stat-roll affixes)
    /// or the embedded behavior (for behavior affixes). <see cref="Roll"/> samples a value (per
    /// the asset's own rules) and calls Instantiate. Callers use Instantiate when they have an
    /// explicit value (fixed talents, replayed rolls from save data) and Roll when they're
    /// generating fresh rolls (loot drops).</para>
    /// </remarks>
    public abstract class NamedModifier : ScriptableObject {
        [Tooltip("Stable registry key, snake_case convention (e.g. \"iron_will\", \"life_t1\"). " +
                 "Hashed to a uint for compact wire / save encoding. Must be unique across all " +
                 "NamedModifier assets — the registry asserts no collisions at load.")]
        [SerializeField] private string key;

        [Tooltip("Player-facing display name (e.g. \"Iron Will\", \"+10 Life\"). Shown in " +
                 "tooltips. Falls back to Key if left empty.")]
        [SerializeField] private string displayName;

        public string Key => key;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? key : displayName;

        /// <summary>
        /// Build a runtime <see cref="SbModifier"/> instance for this affix using the supplied
        /// value (for stat-roll affixes — Flat amount, Increased percent, etc.). Behavior affixes
        /// ignore <paramref name="value"/> and return a clone of their embedded behavior. The
        /// returned modifier's UniqueId is freshly generated at construction — used for
        /// Apply/Remove tracking within the current process; never serialized.
        /// </summary>
        public abstract SbModifier Instantiate(float value);

        /// <summary>
        /// Sample a value (per the asset's own rules) and produce an <see cref="SbModifier"/>
        /// instance via <see cref="Instantiate"/>. Used by drop generators / random rollers.
        /// Fixed-value callers (talents) skip this and call Instantiate directly with their
        /// authored value.
        /// </summary>
        public abstract SbModifier Roll(System.Random rng);
    }
}
