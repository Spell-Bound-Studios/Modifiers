// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// The shield eats damage before anything else: drains the owner's shield pool by the incoming total and
    /// scales every entry down by the absorbed fraction, preserving the type mix for the leaves downstream.
    /// The pool lives on the controller, so the drain seam is a delegate that returns what was absorbed.
    /// </summary>
    public sealed class AbsorptionLeaf : ModifierLeaf {
        private readonly Func<float, float> _absorb;

        public AbsorptionLeaf(Func<float, float> absorb) => _absorb = absorb;

        public override void Process(CircuitContext ctx) {
            var packet = ctx.Packet;

            if (packet == null)
                return;

            var total = 0f;

            for (var i = 0; i < packet.Count; i++)
                total += packet[i].amount;

            if (total <= 0f)
                return;

            var absorbed = _absorb(total);

            if (absorbed <= 0f)
                return;

            var factor = Mathf.Max(0f, (total - absorbed) / total);

            for (var i = 0; i < packet.Count; i++) {
                var entry = packet[i];
                packet[i] = new StatAndValue(entry.statHash, entry.amount * factor);
            }
        }

        public override string ToString() => "absorption";
    }
}
