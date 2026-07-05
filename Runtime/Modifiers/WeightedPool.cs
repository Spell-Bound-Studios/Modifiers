// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    public abstract class WeightedPool<T> : ScriptableObject where T : class {
        [SerializeField] private List<WeightedEntry<T>> entries = new();

        public IReadOnlyList<WeightedEntry<T>> Entries => entries;

        public List<T> Sample(int count, System.Random rng, bool withReplacement = false) {
            var result = new List<T>(Math.Max(count, 0));

            if (count <= 0 || entries == null || entries.Count == 0)
                return result;

            var taken = new bool[entries.Count];

            for (var pick = 0; pick < count; pick++) {
                var totalWeight = 0;

                for (var i = 0; i < entries.Count; i++) {
                    if (!taken[i] && entries[i].candidate != null && entries[i].weight > 0)
                        totalWeight += entries[i].weight;
                }

                if (totalWeight <= 0)
                    break;

                var roll = rng.Next(totalWeight);
                var cumulative = 0;

                for (var i = 0; i < entries.Count; i++) {
                    if (taken[i] || entries[i].candidate == null || entries[i].weight <= 0)
                        continue;

                    cumulative += entries[i].weight;

                    if (roll < cumulative) {
                        if (!withReplacement)
                            taken[i] = true;

                        result.Add(entries[i].candidate);

                        break;
                    }
                }
            }

            return result;
        }
    }
}