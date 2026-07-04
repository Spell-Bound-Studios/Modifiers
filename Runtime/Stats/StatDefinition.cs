// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Registries;
using Spellbound.Core.Tooling;
using UnityEngine;

namespace Spellbound.Modifiers {
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Stat Definition")]
    public class StatDefinition : HashedScriptableObject {
        [Header("Identity"), SerializeField] private string statName;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField, SpritePreview] private Sprite icon;

        public string StatName => statName;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? statName : displayName;
        public string Description => description;
        public Sprite Icon => icon;
    }
}
