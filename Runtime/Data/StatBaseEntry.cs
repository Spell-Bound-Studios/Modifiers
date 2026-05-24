// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored stat seed: a <see cref="StatDefinition"/> asset paired with the base value the
    /// owning behaviour starts with. One entry mirrors one call to <c>SetBase(stat.StatName, baseValue)</c>.
    /// </summary>
    [Serializable, InlineTemplate]
    public struct StatBaseEntry {
        public StatDefinition stat;
        public float baseValue;

        public override string ToString() {
            if (stat == null)
                return $"(no stat): {baseValue:G}";

            var name = string.IsNullOrEmpty(stat.DisplayName) ? stat.StatName : stat.DisplayName;

            return $"{name}: {baseValue:G}";
        }
    }
}
