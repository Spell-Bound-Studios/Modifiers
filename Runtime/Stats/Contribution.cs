// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    public readonly struct Contribution {
        public const uint Innate = 0;

        public readonly ContributionType Type;
        public readonly int ValueInternal;
        public readonly uint SourceId;
        public readonly Condition Condition;

        public Contribution(ContributionType type, int valueInternal, uint sourceId, Condition condition) {
            Type = type;
            ValueInternal = valueInternal;
            SourceId = sourceId;
            Condition = condition;
        }

        public bool IsConditional => Condition != null;

        public static Contribution Of(
                ContributionType type, float value, uint sourceId = Innate, Condition condition = null) =>
                new(type, StatSettings.ToInternal(value), sourceId, condition);
    }
}
