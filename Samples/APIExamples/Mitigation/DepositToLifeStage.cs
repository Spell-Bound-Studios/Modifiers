// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// The terminal stage: whatever survived the circuit is drained from the defender's life pool. This is where
    /// the current reaches life.
    /// </summary>
    [Serializable]
    public sealed class DepositToLifeStage : IPipelineStage<DamageContext> {
        public void Process(in DamageContext ctx) {
            var resource = ctx.Defender.GetBehaviour<ResourceBehaviour>();
            resource?.Apply(ctx.Incoming);
        }
    }
}
