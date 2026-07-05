// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Flat-reduces every entry of one damage type by the subject's armor stat, clamped at zero.
    /// </summary>
    public sealed class ArmorLeaf : ModifierLeaf {
        private readonly StatId _damage;
        private readonly StatId _armor;

        public ArmorLeaf(StatId damage, StatId armor) {
            _damage = damage;
            _armor = armor;
        }

        public override void Process(CircuitContext ctx) {
            var packet = ctx.Packet;

            if (packet == null)
                return;

            var armor = ctx.Subject.GetValue(_armor, ctx);

            if (armor <= 0f)
                return;

            for (var i = 0; i < packet.Count; i++) {
                var entry = packet[i];

                if (entry.statHash != _damage.Hash)
                    continue;

                packet[i] = new StatAndValue(entry.statHash, Mathf.Max(0f, entry.amount - armor));
            }
        }

        public override string ToString() => "armor";
    }
}