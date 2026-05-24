// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// One row inside an <see cref="SbBehaviour"/>'s runtime modifier list: which stat is affected (by
    /// registry id), how the value combines (<see cref="ModifierType.Flat"/> / <see cref="ModifierType.Increased"/>
    /// / <see cref="ModifierType.More"/> / <see cref="ModifierType.Override"/>), the magnitude, and an
    /// optional <see cref="UniqueId"/> the modifier instance carries so it can be removed by identity later.
    /// </summary>
    /// <remarks>
    /// Constructed indirectly through <see cref="SbBehaviour"/>'s name-keyed helpers
    /// (<c>AddFlat</c> / <c>AddIncreased</c> / <c>AddMore</c>) and removed via
    /// <see cref="SbBehaviour.RemoveModifierByUniqueId"/>. Game code should never construct one directly — go
    /// through the helpers so name-to-id interning happens.
    /// <para>
    /// Not to be confused with <see cref="ModifierEntry"/>, its inspector-authoring sibling: that struct
    /// carries a <see cref="StatDefinition"/> reference (a designer-picked asset) and is the shape designers
    /// use to bake modifiers into a preset module at edit-time. <see cref="StatModifierEntry"/> is the
    /// runtime form — a plain <c>int</c> id keyed by <see cref="StatRegistry"/>, no asset references — that
    /// lives inside the dictionary <see cref="SbBehaviour"/> reads during <see cref="SbBehaviour.GetValue(int)"/>.
    /// </para>
    /// </remarks>
    public readonly struct StatModifierEntry {
        public readonly int StatId;
        public readonly ModifierType Type;
        public readonly float Value;
        public readonly string UniqueId;

        public StatModifierEntry(int statId, ModifierType type, float value, string uniqueId = null) {
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