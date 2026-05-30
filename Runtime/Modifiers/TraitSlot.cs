// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Pool slot for named-identity traits. Concrete; lib-side. Designer drops a <see cref="Trait"/>
    /// asset and sampling produces a <see cref="TraitRef"/> wrapping it.
    /// </summary>
    [Serializable]
    public sealed class TraitSlot : PoolSlot {
        [Tooltip("The trait asset this slot rolls. Sampling produces a TraitRef wrapping this " +
                 "trait — Apply clones the trait's effect onto the target.")]
        public Trait Trait;

        public override SbModifier Sample(System.Random rng) {
            if (Trait == null)
                return null;

            return new TraitRef().Initialize(Trait);
        }
    }
}