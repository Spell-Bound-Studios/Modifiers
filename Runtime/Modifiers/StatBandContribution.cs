// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// One mod line spanning the two ends of a range across two stats — "adds (1-3) to (3-5) fire damage"
    /// lands the low end on minimum fire damage and the high end on maximum. Each end takes any magnitude,
    /// so bands compose with rolls and derived scaling ("5 to 10 added fire damage per 25 strength").
    /// </summary>
    [Serializable]
    [SerializeReferenceLabel("Stat Band")]
    public sealed class StatBandContribution : ContributionSpecification {
        [SerializeField, Tooltip("The stat the low end lands on (e.g. minimum fire damage).")]
        private StatDefinition lowStat;

        [SerializeReference, Tooltip("The low end's amount.")]
        private Magnitude lowAmount;

        [SerializeField, Tooltip("The stat the high end lands on (e.g. maximum fire damage).")]
        private StatDefinition highStat;

        [SerializeReference, Tooltip("The high end's amount.")]
        private Magnitude highAmount;

        [SerializeField, Tooltip("Flat adds raw amounts. Increased / More scale by a percent. Override forces the values. Applies to both ends.")]
        private ContributionType contributionType;

        [SerializeField, Tooltip("Never let the high end bake below the low end.")]
        private bool keepOrdered = true;

        public StatDefinition LowStat => lowStat;
        public Magnitude LowAmount => lowAmount;
        public StatDefinition HighStat => highStat;
        public Magnitude HighAmount => highAmount;
        public ContributionType Type => contributionType;
        public bool KeepOrdered => keepOrdered;

        public override bool IsValid =>
                lowStat != null && highStat != null && lowStat != highStat &&
                lowAmount != null && lowAmount.IsValid && highAmount != null && highAmount.IsValid &&
                (!keepOrdered || OrderingEnforceable);

        private bool OrderingEnforceable {
            get {
                if (lowAmount is not ScalarMagnitude lowScalar || highAmount is not ScalarMagnitude highScalar)
                    return false;

                if (lowAmount.Rolls || highAmount.Rolls)
                    return true;

                return lowScalar.Value(0f) <= highScalar.Value(0f);
            }
        }

        public override IEnumerable<(StatDefinition stat, ContributionType type, Magnitude amount)>
                StatContributions {
            get {
                yield return (lowStat, contributionType, lowAmount);
                yield return (highStat, contributionType, highAmount);
            }
        }

        public override void Bake(System.Random rng, List<BakedRoll> into) {
            if (lowStat == null || highStat == null || lowAmount == null || highAmount == null)
                return;

            var lowRolls = lowAmount.Rolls;
            var highRolls = highAmount.Rolls;

            if (!lowRolls && !highRolls)
                return;

            var low = lowRolls ? lowAmount.Bake(rng) : 0f;
            var high = highRolls ? highAmount.Bake(rng) : 0f;

            if (keepOrdered) {
                var effectiveLow = EffectiveValue(lowAmount, low);
                var effectiveHigh = EffectiveValue(highAmount, high);

                if (effectiveHigh < effectiveLow) {
                    if (highRolls)
                        high = effectiveLow;
                    else if (lowRolls)
                        low = effectiveHigh;
                }
            }

            if (lowRolls)
                into.Add(new BakedRoll { statHash = lowStat.Hash, value = low });

            if (highRolls)
                into.Add(new BakedRoll { statHash = highStat.Hash, value = high });
        }

        public override void ApplyBaked(StatBlock stats, uint sourceId, BakedRoll[] baked) {
            if (lowStat != null && lowAmount != null) {
                lowAmount.ApplyTo(stats, new StatId(lowStat.Hash), contributionType, sourceId,
                        BakedValueFor(baked, lowStat.Hash));
            }

            if (highStat != null && highAmount != null) {
                highAmount.ApplyTo(stats, new StatId(highStat.Hash), contributionType, sourceId,
                        BakedValueFor(baked, highStat.Hash));
            }
        }

        private static float EffectiveValue(Magnitude magnitude, float baked) =>
                magnitude is ScalarMagnitude scalar ? scalar.Value(baked) : baked;
    }
}
