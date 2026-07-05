// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers.Samples {
    public sealed class KillingBlowLeaf : ModifierLeaf {
        private readonly Func<bool> _isDead;

        public KillingBlowLeaf(Func<bool> isDead) => _isDead = isDead;

        public override void Process(CircuitContext ctx) {
            if (_isDead())
                ctx.Note(DemoConsequences.KillingBlow, 1f);
        }

        public override string ToString() => "killing-blow";
    }
}
