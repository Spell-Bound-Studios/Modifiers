// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers {
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Modifier Pool")]
    public sealed class ModifierPool : WeightedPool<ModifierDefinition> {
    }
}
