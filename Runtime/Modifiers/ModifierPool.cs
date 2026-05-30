// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored pool of modifier roll templates with per-entry weights. Drop generators
    /// sample N entries from a pool by weight and produce fresh <see cref="SbModifier"/> instances
    /// ready to encode into an item's data byte[].
    /// </summary>
    /// <remarks>
    /// Slots are polymorphic — lib ships abstract <see cref="AffixSlot"/> and concrete
    /// <see cref="TraitSlot"/>; consumers ship a concrete <see cref="AffixSlot"/> subclass that
    /// picks the game's concrete <see cref="Affix"/> type. Selection among slots is
    /// weight-proportional; higher <c>Weight</c> = higher pick probability.
    /// </remarks>
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Modifier Pool")]
    public class ModifierPool : ScriptableObject {
        [SerializeReference, DropdownPicker]
        private List<PoolSlot> slots = new();

        public IReadOnlyList<PoolSlot> Slots => slots;

        /// <summary>
        /// Sample <paramref name="count"/> entries from the pool, weight-proportional with
        /// replacement. Each pick rolls a random value in <c>[0, totalWeight)</c>, walks slots
        /// accumulating weight until the running total crosses the roll, then calls that slot's
        /// <see cref="PoolSlot.Sample"/> to produce a fresh <see cref="SbModifier"/> instance.
        /// Returns an empty list if the pool has no slots with positive weight.
        /// </summary>
        /// <remarks>
        /// "With replacement" means the same slot can be picked multiple times in one call — fine
        /// for stat-flavor pools where each pick rolls a fresh value, but caller is responsible for
        /// deduping if it wants distinct stats per pick. Slots with <c>Weight == 0</c> are skipped
        /// (treat zero as "disabled, leave in the asset for reference").
        /// </remarks>
        public List<SbModifier> Sample(int count, System.Random rng) {
            var result = new List<SbModifier>(Math.Max(count, 0));

            if (count <= 0 || slots == null || slots.Count == 0)
                return result;

            var totalWeight = 0;

            foreach (var slot in slots) {
                if (slot != null && slot.Weight > 0)
                    totalWeight += slot.Weight;
            }

            if (totalWeight <= 0)
                return result;

            for (var i = 0; i < count; i++) {
                var roll = rng.Next(totalWeight);
                var cumulative = 0;

                foreach (var slot in slots) {
                    if (slot == null || slot.Weight <= 0)
                        continue;

                    cumulative += slot.Weight;

                    if (roll < cumulative) {
                        var sampled = slot.Sample(rng);

                        if (sampled != null)
                            result.Add(sampled);

                        break;
                    }
                }
            }

            return result;
        }
    }
}
