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
        [SerializeField] private List<ContributionRange> contributions = new();

        public string ModifierName => modifierName;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? modifierName : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public IReadOnlyList<ContributionRange> Contributions => contributions;

        public RolledModifier Roll(System.Random rng, uint sourceId) {
            var values = new float[contributions.Count];

            for (var i = 0; i < contributions.Count; i++)
                values[i] = contributions[i].Roll(rng);

            return new RolledModifier { modifierHash = Hash, sourceId = sourceId, values = values };
        }
    }
}