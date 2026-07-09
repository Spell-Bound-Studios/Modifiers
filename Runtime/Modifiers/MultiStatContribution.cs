// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// One mod line whose single amount lands on several stats at once — "+10-20 to all resistances" rolls
    /// ONCE and every listed stat receives that same value. Distinct from authoring one line per stat, which
    /// would roll each stat independently.
    /// </summary>
    [Serializable]
    [SerializeReferenceLabel("Multiple Stats")]
    public sealed class MultiStatContribution : ContributionSpecification {
        [SerializeField, Tooltip("Every stat in this list receives the same amount.")]
        private List<StatDefinition> stats = new();

        [SerializeField, Tooltip("Flat adds a raw amount. Increased / More scale by a percent. Override forces the value. Applies to every listed stat.")]
        private ContributionType contributionType;

        [SerializeReference, Tooltip("How the shared amount is decided. A roll happens once; every stat gets the same value.")]
        private Magnitude amount;

        public IReadOnlyList<StatDefinition> Stats => stats;
        public ContributionType Type => contributionType;
        public Magnitude Amount => amount;

        public override bool IsValid {
            get {
                if (stats == null || stats.Count == 0 || amount == null || !amount.IsValid)
                    return false;

                for (var i = 0; i < stats.Count; i++) {
                    if (stats[i] == null)
                        return false;

                    for (var j = i + 1; j < stats.Count; j++) {
                        if (stats[i] == stats[j])
                            return false;
                    }
                }

                return true;
            }
        }

        public override IEnumerable<(StatDefinition stat, Magnitude amount)> Lines {
            get {
                for (var i = 0; i < stats.Count; i++)
                    yield return (stats[i], amount);
            }
        }

        public override void Bake(System.Random rng, List<BakedRoll> into) {
            if (amount == null || !amount.Rolls)
                return;

            var value = amount.Bake(rng);

            for (var i = 0; i < stats.Count; i++) {
                if (stats[i] != null)
                    into.Add(new BakedRoll { statHash = stats[i].Hash, value = value });
            }
        }

        public override void ApplyBaked(StatBlock statBlock, uint sourceId, BakedRoll[] baked) {
            if (amount == null)
                return;

            for (var i = 0; i < stats.Count; i++) {
                if (stats[i] == null)
                    continue;

                amount.ApplyTo(statBlock, new StatId(stats[i].Hash), contributionType, sourceId,
                        BakedValueFor(baked, stats[i].Hash));
            }
        }
    }
}
