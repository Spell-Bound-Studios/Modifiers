// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// One row in a target's per-instance mitigation table. Keyed by defense stat — one row declares "this
    /// defensive stat reduces these damage types using this math." A single target can hold many rows;
    /// composition is achieved by overlapping coverage (e.g. <c>armor</c> and <c>endurance</c> both list
    /// <c>physical_damage</c> in their coverage, both rows fire, math chains in row order).
    /// </summary>
    /// <remarks>
    /// <see cref="UniqueId"/> lets a modifier remove exactly the row it added (mirrors the
    /// <c>RemoveModifierByUniqueId</c> pattern on stats). Vanilla rows baked from a template carry no id.
    /// </remarks>
    public readonly struct MitigationRow {
        public readonly uint DefenseStatId;
        public readonly uint[] CoveredDamageStatIds;
        public readonly IMitigationStrategy Strategy;
        public readonly string UniqueId;

        public MitigationRow(
            uint defenseStatId, uint[] coveredDamageStatIds, IMitigationStrategy strategy,
            string uniqueId) {
            DefenseStatId = defenseStatId;
            CoveredDamageStatIds = coveredDamageStatIds;
            Strategy = strategy;
            UniqueId = uniqueId;
        }
    }
}
