// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// True when its child condition is not met.
    /// </summary>
    public sealed class Not : Condition {
        private readonly Condition _condition;

        public Not(Condition condition) {
            _condition = condition;
        }

        public override bool Met(CircuitContext ctx) => !_condition.Met(ctx);
    }
}