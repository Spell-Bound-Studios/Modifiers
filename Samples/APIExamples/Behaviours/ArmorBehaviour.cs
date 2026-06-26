// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour: owns <c>armor</c>, the flat physical-mitigation stat. A capability an entity has or
    /// doesn't — a golem has it, a wisp doesn't — so a <c>+armor</c> modifier lands here and nowhere else.
    /// </summary>
    [Serializable]
    public sealed class ArmorBehaviour : SbBehaviour {
        public override IReadOnlyList<StatAndValue> DeclareOwnedStats() => new[] { OwnedStat("armor", 10f) };
    }
}
