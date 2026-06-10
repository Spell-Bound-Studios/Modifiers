// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// One row inside an SbBehaviour's runtime modifier list: which stat (by hash), how the value combines,
    /// the magnitude, and an optional UniqueId so the modifier can be removed by identity later.
    /// </summary>
    public readonly struct StatModifierEntry {
        public readonly uint StatHash;
        public readonly ModifierType Type;
        public readonly float Value;
        public readonly string UniqueId;

        public StatModifierEntry(uint statHash, ModifierType type, float value, string uniqueId = null) {
            StatHash = statHash;
            Type = type;
            Value = value;
            UniqueId = uniqueId;
        }

        public override string ToString() {
            var name = StatRegistry.TryGetName(StatHash, out var n) ? n : $"#{StatHash}";

            return string.IsNullOrEmpty(UniqueId)
                    ? $"{name} {Type}({Value:G})"
                    : $"{name} {Type}({Value:G}) [{UniqueId}]";
        }
    }
}
