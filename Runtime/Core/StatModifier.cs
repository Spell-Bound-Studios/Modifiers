namespace Spellbound.Stats
{
    public readonly struct StatModifier
    {
        public readonly int StatId;           // What?
        public readonly ModifierType Type;    // How?
        public readonly float Value;          // Amount?
        public readonly int SourceId;         // Who or where it came from???
        public readonly ulong TagMask;        // Conditional
        
        public StatModifier(int statId, ModifierType type, float value, int sourceId = 0, ulong tagMask = 0)
        {
            StatId = statId;
            Type = type;
            Value = value;
            SourceId = sourceId;
            TagMask = tagMask;
        }
    }
}