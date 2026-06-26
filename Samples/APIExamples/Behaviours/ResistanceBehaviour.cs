// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour: owns the three elemental resistances. One behaviour, three stats — they behave
    /// identically, just on different damage types, which is exactly why they share a behaviour.
    /// </summary>
    [Serializable]
    public sealed class ResistanceBehaviour : SbBehaviour {
        public override IReadOnlyList<StatAndValue> DeclareOwnedStats() => new[] {
            OwnedStat("fire_resistance", 20f),
            OwnedStat("cold_resistance", 20f),
            OwnedStat("lightning_resistance", 20f)
        };
    }
}
