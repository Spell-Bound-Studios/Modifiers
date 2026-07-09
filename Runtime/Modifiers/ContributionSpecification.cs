// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Modifiers {
    /// <summary>
    /// One line of stat changes inside a <see cref="ModifierDefinition"/> or a <see cref="ModifierGrantSet"/>:
    /// which stat(s) it touches and how the amounts are decided. Subclasses define the shapes — single stat,
    /// low-high band, shared multi-stat — and new shapes are added by extending this class; the inspector
    /// picker lists subclasses automatically. Rolled magnitudes bake once per owning instance into stat-keyed
    /// <see cref="BakedRoll"/>s.
    /// </summary>
    [Serializable]
    public abstract class ContributionSpecification : ModifierGrant {
        /// <summary>
        /// The elementary stat contributions this shape decomposes into — stat, contribution type, amount.
        /// The registry validates rolled-stat uniqueness through it, and shape-agnostic consumers (tooltips,
        /// tooling) can render any shape from it without a per-shape case.
        /// </summary>
        public abstract IEnumerable<(StatDefinition stat, ContributionType type, Magnitude amount)>
                StatContributions { get; }

        public abstract void Bake(System.Random rng, List<BakedRoll> into);

        public abstract void ApplyBaked(StatBlock stats, uint sourceId, BakedRoll[] baked);

        public sealed override void Roll(
            System.Random rng, uint sourceId, List<BakedRoll> baked, List<RolledModifier> modifiers) =>
                Bake(rng, baked);

        public sealed override void Apply(Modifiable target, uint sourceId, in RolledGrants rolled) =>
                ApplyBaked(target.Stats, sourceId, rolled.baked);

        protected static float BakedValueFor(BakedRoll[] baked, uint statHash) {
            if (baked != null) {
                for (var i = 0; i < baked.Length; i++) {
                    if (baked[i].statHash == statHash)
                        return baked[i].value;
                }
            }

            return 0f;
        }
    }
}
