// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    public sealed class StatAtLeast : Condition {
        private readonly StatId _stat;
        private readonly float _threshold;

        public StatAtLeast(StatId stat, float threshold) {
            _stat = stat;
            _threshold = threshold;
        }

        public override bool Met(CircuitContext ctx) {
            var owner = ctx.Owner ?? ctx.Subject;

            return owner != null && owner.GetValue(_stat, ctx) >= _threshold;
        }
    }
}
