// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Sampling;
using UnityEngine;

namespace Spellbound.Modifiers {
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Modifier Table")]
    public sealed class ModifierTable : WeightedTableAsset<ModifierDefinition> {
    }
}
