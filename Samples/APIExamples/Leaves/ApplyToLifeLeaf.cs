// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// The terminal stage of a take-hit circuit: whatever survived mitigation is drained from the subject's
    /// life pool. Current health lives on the controller, so the world-effect seam is a delegate.
    /// </summary>
    public sealed class ApplyToLifeLeaf : ActionLeaf {
        private readonly Action<float> _drain;

        public ApplyToLifeLeaf(Action<float> drain) {
            _drain = drain;
        }

        public override void Process(CircuitContext ctx) {
            var packet = ctx.Packet;

            if (packet == null)
                return;

            var total = 0f;

            for (var i = 0; i < packet.Count; i++)
                total += packet[i].amount;

            if (total > 0f)
                _drain(total);
        }

        public override string ToString() => "deposit";
    }
}