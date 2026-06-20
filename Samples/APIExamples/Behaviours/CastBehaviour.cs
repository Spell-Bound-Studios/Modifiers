// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour: the cast aspect of a skill. Owns <c>cast_time</c>; a real skill would also pay costs
    /// and gate on cooldown here. The skill's <c>OnExecute</c> reads it when the cast goes off.
    /// </summary>
    [Serializable]
    public sealed class CastBehaviour : SbBehaviour {
        private static uint? _castTimeHash;
        private static uint CastTimeHash => _castTimeHash ??= StatRegistry.GetHash("cast_time");

        public override IReadOnlyList<StatAndValue> DeclareOwnedStats() => new[] { OwnedStat("cast_time", 1f) };

        public float CastTime => GetValue(CastTimeHash);
    }
}
