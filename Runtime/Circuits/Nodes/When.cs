// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// A decorator that descends into its child only when its condition is met.
    /// </summary>
    public sealed class When : Decorator {
        private readonly Condition _condition;

        public When(Condition condition, Node child) : base(child) {
            _condition = condition;
        }

        public override void Process(CircuitContext ctx) {
            if (_condition.Met(ctx))
                Child.Process(ctx);
        }
    }
}