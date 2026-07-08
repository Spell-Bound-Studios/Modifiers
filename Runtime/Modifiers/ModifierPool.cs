// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Modifier Pool")]
    public sealed class ModifierPool : WeightedPool<ModifierDefinition> {
        public List<RolledModifier> Roll(int count, System.Random rng, bool withReplacement = false) {
            var definitions = Sample(count, rng, withReplacement);
            var result = new List<RolledModifier>(definitions.Count);

            for (var i = 0; i < definitions.Count; i++)
                result.Add(definitions[i].Roll(rng, ModifierSource.Next()));

            return result;
        }
    }
}