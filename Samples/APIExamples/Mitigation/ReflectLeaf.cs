// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Reflects a fraction of one incoming damage type by reporting it on the consequence channel — the packet
    /// is never touched, so the defender still takes the full hit. Granted into the Convert stage so it reads
    /// pre-mitigation values. The sender finds the outcome in the consequence and applies it to itself; a
    /// reflected hit's consequence is never forwarded, which is the one-bounce guard.
    /// </summary>
    public sealed class ReflectLeaf : ModifierLeaf {
        private readonly StatId _damage;
        private readonly float _fraction;
        private readonly uint _consequenceId;

        public ReflectLeaf(StatId damage, float fraction, uint consequenceId) {
            _damage = damage;
            _fraction = fraction;
            _consequenceId = consequenceId;
        }

        public override void Process(CircuitContext ctx) {
            var packet = ctx.Packet;

            if (packet == null)
                return;

            var reflected = 0f;

            for (var i = 0; i < packet.Count; i++) {
                var entry = packet[i];

                if (entry.statHash == _damage.Hash)
                    reflected += entry.amount;
            }

            if (reflected <= 0f)
                return;

            ctx.Note(_consequenceId, reflected * _fraction);
        }

        public override string ToString() => $"reflect-{DemoNames.Short(_damage)}";
    }
}
