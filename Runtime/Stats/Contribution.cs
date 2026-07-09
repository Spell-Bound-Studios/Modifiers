// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    public readonly struct Contribution {
        public const uint None = 0;

        public readonly ContributionType Type;
        public readonly int ValueInternal;
        public readonly uint SourceId;
        public readonly Condition Condition;
        public readonly StatId SourceStat;
        public readonly float Amount;
        public readonly int PerPoints;
        public readonly bool Stepped;
        public readonly Perspective Perspective;

        public Contribution(
            ContributionType type, int valueInternal, uint sourceId, Condition condition,
            StatId sourceStat = default, float amount = 0f, int perPoints = 1, bool stepped = false,
            Perspective perspective = Perspective.Owner) {
            Type = type;
            ValueInternal = valueInternal;
            SourceId = sourceId;
            Condition = condition;
            SourceStat = sourceStat;
            Amount = amount;
            PerPoints = perPoints;
            Stepped = stepped;
            Perspective = perspective;
        }

        public bool IsConditional => Condition != null;

        public bool IsDerived => SourceStat.Hash != 0;

        public static Contribution Of(
            ContributionType type, float value, uint sourceId = None, Condition condition = null) =>
                new(type, StatSettings.ToInternal(value), sourceId, condition);

        public static Contribution Derived(
            ContributionType type, StatId sourceStat, float amount, int perPoints, bool stepped,
            Perspective perspective, uint sourceId = None, Condition condition = null) =>
                new(type, 0, sourceId, condition, sourceStat, amount, perPoints, stepped, perspective);
    }
}
