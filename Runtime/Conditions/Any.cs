// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// True when any child condition is met.
    /// </summary>
    public sealed class Any : Condition {
        private readonly Condition[] _conditions;

        public Any(params Condition[] conditions) {
            _conditions = conditions;
        }

        public override bool Met(CircuitContext ctx) {
            for (var i = 0; i < _conditions.Length; i++) {
                if (_conditions[i].Met(ctx))
                    return true;
            }

            return false;
        }
    }
}