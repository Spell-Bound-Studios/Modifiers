// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Percent-reduces one damage type by its matching resistance stat (5 = 5% off), capped at 100%. One leaf,
    /// three instances — fire / cold / lightning behave identically, just on different stats. The stat read
    /// passes the live context through, so a conditional resistance modifier can see the hit it's judging.
    /// </summary>
    public sealed class ResistanceLeaf : ModifierLeaf {
        private readonly StatId _damage;
        private readonly StatId _resistance;

        public ResistanceLeaf(StatId damage, StatId resistance) {
            _damage = damage;
            _resistance = resistance;
        }

        public override void Process(CircuitContext ctx) {
            var packet = ctx.Packet;

            if (packet == null)
                return;

            var resistance = ctx.Subject.Stats.GetValue(_resistance, ctx);

            if (resistance <= 0f)
                return;

            var multiplier = Mathf.Max(0f, 1f - resistance / 100f);

            for (var i = 0; i < packet.Count; i++) {
                var entry = packet[i];

                if (entry.statHash != _damage.Hash)
                    continue;

                packet[i] = new StatAndValue(entry.statHash, entry.amount * multiplier);
            }
        }

        public override string ToString() => DemoNames.Short(_damage);
    }
}
