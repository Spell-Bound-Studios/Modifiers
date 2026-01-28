namespace Spellbound.Stats {
    /// <summary>
    /// This is a data transfer object so that we can easily modify our stat container without passing in a long tuple.
    /// </summary>
    public readonly struct StatModifier {
        public readonly int StatId;
        public readonly ModifierType Type;
        public readonly float Value;
        public readonly string UniqueId;

        public StatModifier(int statId, ModifierType type, float value, string uniqueId = null) {
            StatId = statId;
            Type = type;
            Value = value;
            UniqueId = uniqueId;
        }
    }
}