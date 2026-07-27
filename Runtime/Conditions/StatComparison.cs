// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    public sealed class StatComparison : Condition {
        private readonly StatId _left;
        private readonly Perspective _leftPerspective;
        private readonly ComparisonOperator _comparison;
        private readonly StatId _right;
        private readonly Perspective _rightPerspective;
        private readonly float _offset;

        public StatComparison(
            StatId left, Perspective leftPerspective, ComparisonOperator comparison, StatId right,
            Perspective rightPerspective, float offset = 0f) {
            _left = left;
            _leftPerspective = leftPerspective;
            _comparison = comparison;
            _right = right;
            _rightPerspective = rightPerspective;
            _offset = offset;
        }

        public override bool Met(CircuitContext ctx) {
            var leftEntity = EntityFor(_leftPerspective, ctx);
            var rightEntity = EntityFor(_rightPerspective, ctx);

            if (leftEntity == null || rightEntity == null)
                return false;

            var left = leftEntity.GetValue(_left, ctx);
            var right = rightEntity.GetValue(_right, ctx) + _offset;

            return _comparison switch {
                ComparisonOperator.LessThan => left < right,
                ComparisonOperator.AtMost => left <= right,
                ComparisonOperator.AtLeast => left >= right,
                ComparisonOperator.GreaterThan => left > right,
                _ => false
            };
        }

        private static Modifiable EntityFor(Perspective perspective, CircuitContext ctx) =>
                perspective == Perspective.Subject ? ctx.Subject : ctx.Owner ?? ctx.Subject;
    }
}
