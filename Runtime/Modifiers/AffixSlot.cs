// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Modifiers {
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

        [Tooltip("Sample range. Math is uniform in [x, y) — y itself is never sampled before " +
                 "Step snapping.")]
        public Vector2 RollRange;

        [Tooltip("Sampled value snaps to multiples of step. 1 for integer stats (armor, damage); " +
                 "0.01 for percent stats (1% precision); 0.0001 for 0.01%-precision rares.")]
        [Min(0.0000001f)] public float Step = 1f;

        public override SbModifier Sample(System.Random rng) {
            if (Stat == null) {
                Log.Error("AffixSlot has no Stat assigned; cannot sample.");

                return null;
            }

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
}
