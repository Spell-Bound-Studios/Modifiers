namespace Spellbound.Stats {
    /// <summary>
    /// This is a data transfer object so that we can easily modify our stat container without passing in a long tuple.
    /// </summary>
    public readonly struct StatModifier {
        public readonly int StatId;
        public readonly ModifierType Type;
        public readonly float Value;
        public readonly int SourceId;

        public StatModifier(int statId, ModifierType type, float value, int sourceId = 0) {
            StatId = statId;
            Type = type;
            Value = value;
            SourceId = sourceId;
        }
    }
}