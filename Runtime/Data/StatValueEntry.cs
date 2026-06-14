// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored (stat, value) pair: a <see cref="StatDefinition"/> reference plus an amount. The
    /// authoring twin of the runtime <see cref="StatAndValue"/> — read <c>stat.Hash</c> at build / send time.
    /// </summary>
    [Serializable, InlineTemplate]
    public struct StatValueEntry {
        public StatDefinition stat;
        public float value;

        public override string ToString() {
            if (stat == null)
                return $"(no stat): {value:G}";

            var name = string.IsNullOrEmpty(stat.DisplayName) ? stat.StatName : stat.DisplayName;

            return $"{name}: {value:G}";
        }
    }
}
