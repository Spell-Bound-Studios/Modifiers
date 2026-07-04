// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    public sealed class AbsorptionLeaf : ModifierLeaf {
        private readonly Func<float, float> _absorb;
        private readonly StatId _bypassDamage;
        private readonly StatId _bypassPercent;

        public AbsorptionLeaf(Func<float, float> absorb, StatId bypassDamage, StatId bypassPercent) {
            _absorb = absorb;
            _bypassDamage = bypassDamage;
            _bypassPercent = bypassPercent;
        }

        public override void Process(CircuitContext ctx) {
            var packet = ctx.Packet;

            if (packet == null)
                return;

            var bypass = Mathf.Clamp(ctx.Subject.GetValue(_bypassPercent, ctx), 0f, 100f) / 100f;
            var absorbable = 0f;

            for (var i = 0; i < packet.Count; i++) {
                var entry = packet[i];
                absorbable += entry.statHash == _bypassDamage.Hash ? entry.amount * (1f - bypass) : entry.amount;
            }

            if (absorbable <= 0f)
                return;

            var absorbed = _absorb(absorbable);

            if (absorbed <= 0f)
                return;

            var factor = Mathf.Max(0f, (absorbable - absorbed) / absorbable);

            for (var i = 0; i < packet.Count; i++) {
                var entry = packet[i];

                if (entry.statHash == _bypassDamage.Hash)
                    packet[i] = new StatAndValue(entry.statHash,
                            entry.amount * bypass + entry.amount * (1f - bypass) * factor);
                else
                    packet[i] = new StatAndValue(entry.statHash, entry.amount * factor);
            }
        }

        public override string ToString() => "absorption";
    }
}
