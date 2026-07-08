// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Registries;
using UnityEngine;

namespace Spellbound.Modifiers {
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Modifier Definition")]
    public class ModifierDefinition : HashedScriptableObject {
        [Header("Identity"), SerializeField] private string modifierName;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField, SpritePreview] private Sprite icon;
        [SerializeField] private List<ContributionSpecification> contributions = new();

        public string ModifierName => modifierName;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? modifierName : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public IReadOnlyList<ContributionSpecification> Contributions => contributions;

        public RolledModifier Roll(System.Random rng, uint sourceId) {
            var baked = new List<BakedRoll>();

            for (var i = 0; i < contributions.Count; i++) {
                var spec = contributions[i];
                var low = 0f;
                var lowBaked = false;

                if (spec.Stat != null && spec.Magnitude != null && spec.Magnitude.Rolls) {
                    low = spec.Magnitude.Bake(rng);
                    baked.Add(new BakedRoll { statHash = spec.Stat.Hash, value = low });
                    lowBaked = true;
                }

                if (spec.PairedStat != null && spec.PairedMagnitude != null && spec.PairedMagnitude.Rolls) {
                    var high = spec.PairedMagnitude.Bake(rng);

                    if (spec.LinkOrdered && lowBaked && high < low)
                        high = low;

                    baked.Add(new BakedRoll { statHash = spec.PairedStat.Hash, value = high });
                }
            }

            return new RolledModifier { modifierHash = Hash, sourceId = sourceId, baked = baked.ToArray() };
        }
    }
}
