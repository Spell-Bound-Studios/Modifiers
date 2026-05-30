// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Abstract base for one entry in a <see cref="ModifierPool"/>. Concrete subclasses
    /// (<see cref="TraitSlot"/> in lib; the game's <c>StatAffixSlot</c> or equivalent) carry their
    /// type-specific fields and implement <see cref="Sample"/> to produce a fresh
    /// <see cref="SbModifier"/> instance.
    /// </summary>
    [Serializable]
    public abstract class PoolSlot {
        [Tooltip("Selection weight — higher = picked more often when sampling from the pool. " +
                 "Only the ratio between slots matters: 100/100 = 50/50; 1000/10 = ~99%/~1%.")]
        [Min(0)] public int Weight;

        /// <summary>
        /// Produce a fresh <see cref="SbModifier"/> instance from this slot. Stat-flavor slots
        /// sample a value in their roll range and snap to step; trait slots clone the trait's
        /// effect. Caller chains the result into a list passed to <c>ModifierCodec.Encode</c>.
        /// </summary>
        public abstract SbModifier Sample(System.Random rng);
    }
}
