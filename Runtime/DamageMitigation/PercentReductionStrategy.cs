// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// <c>damage * clamp01(1 - stat / 100)</c>. Useful as a starting point and as the canonical
    /// PoE-elemental-resistance shape. Ships with the library so every project has at least one working
    /// strategy out of the box.
    /// </summary>
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Mitigation/Percent Reduction Strategy")]
    public sealed class PercentReductionStrategy : MitigationStrategy {
        public override float Apply(float incomingDamage, float defensiveStatValue) =>
                incomingDamage * Mathf.Clamp01(1f - defensiveStatValue / 100f);
    }
}