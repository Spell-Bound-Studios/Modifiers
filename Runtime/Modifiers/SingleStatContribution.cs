// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    [Serializable]
    [SerializeReferenceLabel("Single Stat")]
    public sealed class SingleStatContribution : ContributionSpecification {
        [SerializeField, Tooltip("Which stat this line changes.")]
        private StatDefinition stat;

        [SerializeField, Tooltip("Flat adds a raw amount. Increased / More scale by a percent. Override forces the value.")]
        private ContributionType contributionType;

        [SerializeReference, Tooltip("How the amount is decided: a fixed value, a random roll, or scaled from another stat.")]
        private Magnitude amount;

        public StatDefinition Stat => stat;
        public ContributionType Type => contributionType;
        public Magnitude Amount => amount;

        public override bool IsValid => stat != null && amount != null && amount.IsValid;

        public override IEnumerable<(StatDefinition stat, ContributionType type, Magnitude amount)>
                StatContributions {
            get { yield return (stat, contributionType, amount); }
        }

        public override void Bake(System.Random rng, List<BakedRoll> into) {
            if (stat == null || amount == null || !amount.Rolls)
                return;

            into.Add(new BakedRoll { statHash = stat.Hash, value = amount.Bake(rng) });
        }

        public override void ApplyBaked(StatBlock stats, uint sourceId, BakedRoll[] baked) {
            if (stat == null || amount == null)
                return;

            amount.ApplyTo(stats, new StatId(stat.Hash), contributionType, sourceId, BakedValueFor(baked, stat.Hash));
        }
    }
}
