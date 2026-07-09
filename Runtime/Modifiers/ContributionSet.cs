// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Modifiers {
    [Serializable]
    public sealed class ContributionSet {
        [SerializeField] private List<ContributionSpecification> contributions = new();

        public IReadOnlyList<ContributionSpecification> Contributions => contributions;

        /// <summary>
        /// Rolls each contribution and applies it to <paramref name="target"/> under <paramref name="sourceId"/>.
        /// Seed <paramref name="rng"/> deterministically from the owning instance for a stable per-item roll.
        /// </summary>
        public void RollAndApply(Modifiable target, System.Random rng, uint sourceId) {
            for (var i = 0; i < contributions.Count; i++) {
                var specification = contributions[i];

                if (specification == null || !specification.IsValid) {
                    Log.Warn($"ContributionSet: contribution {i} is invalid (needs a stat and magnitude); skipped.");

                    continue;
                }

                specification.RollAndApply(target.Stats, rng, sourceId);
            }
        }
    }
}
