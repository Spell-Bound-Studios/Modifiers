// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour on the attacker side: heals the caster for a fraction of the life damage the defender
    /// actually took. It sums the typed damage the defender returns in its consequence — the post-mitigation
    /// amounts that reached life, so 50 in, 20 armor, 30 to life heals off the 30. The killing-blow signal is
    /// skipped; everything else in the consequence is real damage dealt. <see cref="Fraction"/> is 0 until the
    /// Life Steal modifier turns it on.
    /// </summary>
    [Serializable]
    public sealed class LifeStealBehaviour : SbBehaviour {
        private static uint? _killingBlowHash;
        private static uint KillingBlowHash => _killingBlowHash ??= StatRegistry.GetHash("killing_blow");

        public float Fraction { get; set; }

        /// <summary>The heal earned from a returned consequence: its summed damage × <see cref="Fraction"/>.</summary>
        public float ComputeHeal(List<StatAndValue> consequence) {
            if (Fraction <= 0f)
                return 0f;

            var lifeDamage = 0f;

            foreach (var entry in consequence) {
                if (entry.statHash != KillingBlowHash)
                    lifeDamage += entry.amount;
            }

            return lifeDamage * Fraction;
        }
    }
}
