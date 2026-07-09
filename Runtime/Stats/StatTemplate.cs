// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Modifiers {
    [Serializable]
    public class StatTemplate {
        [SerializeField] private List<BaseStat> baseStats = new();
        [SerializeField] private List<ResourcePoolStat> resourcePools = new();
        [SerializeField] private List<ModifierDefinition> modifiers = new();

        public IReadOnlyList<BaseStat> BaseStats => baseStats;
        public IReadOnlyList<ResourcePoolStat> ResourcePools => resourcePools;
        public IReadOnlyList<ModifierDefinition> Modifiers => modifiers;

        public virtual void ApplyTo(Modifiable target) {
            for (var i = 0; i < baseStats.Count; i++) {
                var entry = baseStats[i];

                if (entry.stat == null) {
                    Log.Warn($"StatTemplate: baseStats[{i}] has no stat assigned; skipped.");

                    continue;
                }

                target.Stats.SetBase(new StatId(entry.stat.Hash), entry.value);
            }
        }
    }
}
