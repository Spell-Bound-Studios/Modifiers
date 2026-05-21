// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// One row inside a <see cref="StatContainer"/>'s modifier list: which stat is affected (by registry id),
    /// how the value combines (<see cref="ModifierType.Flat"/> / <see cref="ModifierType.Increased"/> /
    /// <see cref="ModifierType.More"/> / <see cref="ModifierType.Override"/>), the magnitude, and an optional
    /// <see cref="UniqueId"/> the modifier instance carries so it can be removed by identity later.
    /// </summary>
    /// <remarks>
    /// Authored entirely through <see cref="BehaviourExtensions"/> (<c>AddFlat</c> / <c>AddIncreased</c> /
    /// <c>AddMore</c>) and removed via <see cref="StatContainer.RemoveModifierByUniqueId"/>. Game code should
    /// never construct one directly — go through the extension methods so name-to-id interning happens.
    /// </remarks>
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

        public override string ToString() {
            var name = StatRegistry.TryGetName(StatId, out var n) ? n : $"#{StatId}";

            return string.IsNullOrEmpty(UniqueId)
                    ? $"{name} {Type}({Value:G})"
                    : $"{name} {Type}({Value:G}) [{UniqueId}]";
        }
    }
}