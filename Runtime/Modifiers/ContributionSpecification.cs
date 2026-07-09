// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    [Serializable]
    public sealed class ContributionSpecification {
        [SerializeField] private StatDefinition stat;
        [SerializeField] private ContributionType type;
        [SerializeReference] private Magnitude magnitude;
        [SerializeField] private StatDefinition pairedStat;
        [SerializeReference] private Magnitude pairedMagnitude;
        [SerializeField] private bool linkOrdered;

        public StatDefinition Stat => stat;
        public ContributionType Type => type;
        public Magnitude Magnitude => magnitude;
        public StatDefinition PairedStat => pairedStat;
        public Magnitude PairedMagnitude => pairedMagnitude;
        public bool LinkOrdered => linkOrdered;

        public bool IsValid =>
                stat != null && magnitude != null && magnitude.IsValid &&
                (pairedStat == null || (pairedStat != stat && pairedMagnitude != null && pairedMagnitude.IsValid));

        public void Bake(System.Random rng, List<BakedRoll> into) {
            var low = 0f;
            var lowBaked = false;

            if (stat != null && magnitude != null && magnitude.Rolls) {
                low = magnitude.Bake(rng);
                into.Add(new BakedRoll { statHash = stat.Hash, value = low });
                lowBaked = true;
            }

            if (pairedStat != null && pairedMagnitude != null && pairedMagnitude.Rolls) {
                var high = pairedMagnitude.Bake(rng);

                if (linkOrdered && lowBaked && high < low)
                    high = low;

                into.Add(new BakedRoll { statHash = pairedStat.Hash, value = high });
            }
        }

        public void ApplyBaked(StatBlock stats, uint sourceId, BakedRoll[] baked) {
            if (stat != null && magnitude != null)
                magnitude.ApplyTo(stats, new StatId(stat.Hash), type, sourceId, BakedValueFor(baked, stat.Hash));

            if (pairedStat != null && pairedMagnitude != null)
                pairedMagnitude.ApplyTo(stats, new StatId(pairedStat.Hash), type, sourceId,
                        BakedValueFor(baked, pairedStat.Hash));
        }

        public void RollAndApply(StatBlock stats, System.Random rng, uint sourceId) {
            var low = 0f;
            var lowRolled = false;

            if (stat != null && magnitude != null) {
                if (magnitude.Rolls) {
                    low = magnitude.Bake(rng);
                    lowRolled = true;
                }

                magnitude.ApplyTo(stats, new StatId(stat.Hash), type, sourceId, low);
            }

            if (pairedStat != null && pairedMagnitude != null) {
                var high = 0f;

                if (pairedMagnitude.Rolls) {
                    high = pairedMagnitude.Bake(rng);

                    if (linkOrdered && lowRolled && high < low)
                        high = low;
                }

                pairedMagnitude.ApplyTo(stats, new StatId(pairedStat.Hash), type, sourceId, high);
            }
        }

        private static float BakedValueFor(BakedRoll[] baked, uint statHash) {
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
