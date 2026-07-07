// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers {
    [Serializable]
    public struct ContributionRange {
        public StatDefinition stat;
        public ContributionType type;
        public StatDefinition sourceStat; // null = plain contribution; set = the rolled value is a ratio per point of this stat
        public float min;
        public float max;
        public float step; // roll granularity: 1 rolls integers, 0.5 rolls halves, 0 is continuous

        public float Roll(System.Random rng) {
            var value = min + (float)rng.NextDouble() * (max - min);

            if (step > 0f)
                value = min + Mathf.Round((value - min) / step) * step;

            return Mathf.Clamp(value, min, max);
        }

        public RolledContribution RollContribution(System.Random rng, uint sourceId) =>
                new() {
                    statHash = stat.Hash,
                    type = type,
                    sourceStatHash = sourceStat != null ? sourceStat.Hash : 0u,
                    value = Roll(rng),
                    sourceId = sourceId
                };
    }
}