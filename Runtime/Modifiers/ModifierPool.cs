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

    /// <summary>
    /// Abstract base for one entry in a <see cref="ModifierPool"/>. Concrete subclasses
    /// (<see cref="TraitSlot"/> in lib; the game's <c>StatAffixSlot</c> or equivalent) carry their
    /// type-specific fields and implement <see cref="Sample"/> to produce a fresh
    /// <see cref="SbModifier"/> instance.
    /// </summary>
    [Serializable]
    public abstract class PoolSlot {
        [Tooltip("Selection weight — higher = picked more often when sampling from the pool. " +
                 "Only the ratio between slots matters: 100/100 = 50/50; 1000/10 = ~99%/~1%.")]
        [Min(0)] public int Weight;

        /// <summary>
        /// Produce a fresh <see cref="SbModifier"/> instance from this slot. Stat-flavor slots
        /// sample a value in their roll range and snap to step; trait slots clone the trait's
        /// effect. Caller chains the result into a list passed to <c>ModifierCodec.Encode</c>.
        /// </summary>
        public abstract SbModifier Sample(System.Random rng);
    }

    /// <summary>
    /// Abstract pool slot for anonymous stat-flavor affixes. Carries the roll metadata (Stat,
    /// ModifierType, RollRange, Step) and the rolled-value math. Concrete subclasses override only
    /// <see cref="CreateAffixInstance"/> to specify which concrete <see cref="Affix"/> subtype to
    /// instantiate — that subtype's <c>Apply</c> decides which target behaviour receives the
    /// modifier.
    /// </summary>
    [Serializable]
    public abstract class AffixSlot : PoolSlot {
        [Tooltip("Which stat this slot rolls affixes for.")]
        public StatDefinition Stat;

        [Tooltip("How the rolled value applies — Flat (+X), Increased / More (decimal fraction; " +
                 "0.25 = 25%), Override (last-wins).")]
        public ModifierType ModifierType = ModifierType.Flat;

        [Tooltip("Inclusive value range. Sampled uniform in [x, y].")]
        public Vector2 RollRange;

        [Tooltip("Sampled value snaps to multiples of step. 1 for integer stats (armor, damage); " +
                 "0.01 for percent stats (1% precision); 0.0001 for 0.01%-precision rares.")]
        [Min(0.0000001f)] public float Step = 1f;

        public override SbModifier Sample(System.Random rng) {
            if (Stat == null)
                return null;

            var affix = CreateAffixInstance();

            if (affix == null)
                return null;

            var raw = Mathf.Lerp(RollRange.x, RollRange.y, (float)rng.NextDouble());
            var stepped = Mathf.Round(raw / Step) * Step;

            return affix.Initialize(Stat, ModifierType, stepped);
        }

        /// <summary>
        /// Template method: subclass returns a fresh, unconfigured instance of its concrete
        /// <see cref="Affix"/> subtype. The base <see cref="Sample"/> handles range / step /
        /// configuration. The concrete subtype owns the routing decision (which target behaviour
        /// the rolled affix dispatches to on apply).
        /// </summary>
        protected abstract Affix CreateAffixInstance();
    }

    /// <summary>
    /// Pool slot for named-identity traits. Concrete; lib-side. Designer drops a <see cref="Trait"/>
    /// asset and sampling produces a <see cref="TraitRef"/> wrapping it.
    /// </summary>
    [Serializable]
    public sealed class TraitSlot : PoolSlot {
        [Tooltip("The trait asset this slot rolls. Sampling produces a TraitRef wrapping this " +
                 "trait — Apply clones the trait's effect onto the target.")]
        public Trait Trait;

        public override SbModifier Sample(System.Random rng) {
            if (Trait == null)
                return null;

            return new TraitRef().Initialize(Trait);
        }
    }
}
