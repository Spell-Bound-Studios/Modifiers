// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Registries;
using Spellbound.Core.Tooling;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored asset declaring one stat: its name (the lookup key in <see cref="StatRegistry"/>'s
    /// name index), display name, description, and icon. The stable <see cref="HashedScriptableObject.Hash"/>
    /// identity comes from the asset GUID. Auto-discovered from a Resources/Stats folder by
    /// <see cref="StatRegistry"/>; referenced by preset modules via <see cref="StatBaseEntry"/> /
    /// <see cref="ModifierEntry"/> / <see cref="ResourceBaseEntry"/>.
    /// </summary>
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
