// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored stat seed: a <see cref="StatDefinition"/> asset paired with the base value the
    /// owning behaviour starts with. One entry mirrors one call to <c>SetBase(stat.StatName, baseValue)</c>.
    /// </summary>
    [Serializable]
    public struct StatBaseEntry {
        public StatDefinition stat;
        public float baseValue;
    }
}
