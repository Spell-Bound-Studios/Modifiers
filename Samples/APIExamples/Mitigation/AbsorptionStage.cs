// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// The shield eats damage before anything else: drains the <see cref="AbsorptionBehaviour"/> pool by the
    /// incoming total and scales every entry down by the absorbed fraction, preserving the type mix for the
    /// stages downstream. No <see cref="AbsorptionBehaviour"/> on the defender, no absorption — the stage no-ops.
    /// </summary>
    [Serializable]
    public sealed class AbsorptionStage : IPipelineStage<DamageContext> {
        public void Process(in DamageContext ctx) {
            var absorption = ctx.Defender.GetBehaviour<AbsorptionBehaviour>();

            if (absorption == null)
                return;

            var total = 0f;

            foreach (var entry in ctx.Incoming)
                total += entry.amount;

            if (total <= 0f)
                return;

            var absorbed = absorption.Absorb(total);

            if (absorbed <= 0f)
                return;

            var factor = Math.Max(0f, (total - absorbed) / total);

            for (var i = 0; i < ctx.Incoming.Count; i++) {
                var entry = ctx.Incoming[i];
                ctx.Incoming[i] = new StatAndValue(entry.statHash, entry.amount * factor);
            }
        }
    }
}
