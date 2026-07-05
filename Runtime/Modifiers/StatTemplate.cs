// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    [Serializable]
    public class StatTemplate {
        [SerializeField] private List<BaseStat> baseStats = new();
        [SerializeField] private List<ResourcePoolStat> resourcePools = new();
        [SerializeField] private List<ModifierDefinition> innateModifiers = new();

        public IReadOnlyList<BaseStat> BaseStats => baseStats;
        public IReadOnlyList<ResourcePoolStat> ResourcePools => resourcePools;
        public IReadOnlyList<ModifierDefinition> InnateModifiers => innateModifiers;

        public virtual void ApplyTo(Modifiable target) {
            for (var i = 0; i < baseStats.Count; i++) {
                var entry = baseStats[i];

                if (entry.stat == null)
                    continue;

                target.Stats.SetBase(new StatId(entry.stat.Hash), entry.value);
            }
        }

        public virtual List<RolledModifier> RollInnate(System.Random rng) {
            var result = new List<RolledModifier>(innateModifiers.Count);

            for (var i = 0; i < innateModifiers.Count; i++) {
                if (innateModifiers[i] == null)
                    continue;

                result.Add(innateModifiers[i].Roll(rng, (uint)rng.Next(1, int.MaxValue)));
            }

            return result;
        }
    }
}
