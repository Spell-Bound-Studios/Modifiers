// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored resource declaration. Pairs a <see cref="StatDefinition"/> (the stat that IS the
    /// resource — its computed value is the max) with the base value that seeds the stat and the floor the
    /// resource clamps to. A preset module exposing a list of these declares which resources every instance
    /// spawns with.
    /// </summary>
    /// <remarks>
    /// Three fields, on purpose. Display name / icon / color belong on the backing
    /// <see cref="StatDefinition"/> if anywhere — not here. Current at spawn is always the max; no override.
    /// </remarks>
    [Serializable, InlineTemplate]
    public struct ResourceBaseEntry {
        public StatDefinition stat;
        public float baseValue;
        public float min;

        public override string ToString() {
            if (stat == null)
                return $"(no stat): {baseValue:G} (min {min:G})";

            var name = string.IsNullOrEmpty(stat.DisplayName) ? stat.StatName : stat.DisplayName;

            return $"{name}: {baseValue:G} (min {min:G})";
        }
    }
}
