// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    public readonly struct Contribution {
        public const uint Innate = 0;

        public readonly ContributionType Type;
        public readonly int ValueInternal;
        public readonly uint SourceId;
        public readonly Condition Condition;
        public readonly StatId SourceStat;
        public readonly float RatioPerPoint;

        public Contribution(
            ContributionType type, int valueInternal, uint sourceId, Condition condition,
            StatId sourceStat = default, float ratioPerPoint = 0f) {
            Type = type;
            ValueInternal = valueInternal;
            SourceId = sourceId;
            Condition = condition;
            SourceStat = sourceStat;
            RatioPerPoint = ratioPerPoint;
        }

        public bool IsConditional => Condition != null;

        public bool IsDerived => SourceStat.Hash != 0;

        public static Contribution Of(
            ContributionType type, float value, uint sourceId = Innate, Condition condition = null) =>
                new(type, StatSettings.ToInternal(value), sourceId, condition);

        public static Contribution Derived(
            ContributionType type, StatId sourceStat, float ratioPerPoint, uint sourceId = Innate,
            Condition condition = null) =>
                new(type, 0, sourceId, condition, sourceStat, ratioPerPoint);
    }
}
