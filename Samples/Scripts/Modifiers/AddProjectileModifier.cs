// Copyright 2025 Spellbound Studio Inc.

#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Increases damage for skills with Projectile tag.
    /// Modifies damage stats in damage-dealing behaviors (Fire, Cold, etc.)
    /// </summary>
    [Serializable]
    public class AddProjectileModifier : IModifier {
        [SerializeField] private int amount = 6;

        private int? _modifierId;

        private HashSet<int>? _requiredTags;

        private int _sourceId;
        public int ModifierId => _modifierId ??= GetHashCode();
        public HashSet<int>? RequiredTags => _requiredTags ??= new HashSet<int> { TagRegistry.Register("Projectile") };

        public void Apply(ICanBeModified target) {
            if (!SbModifier.ShouldModifierApply(RequiredTags, target))
                return;

            if (target is not Skill skill)
                return;

            var projectileBehaviour = skill.GetBehaviour<ProjectileBehaviour>();

            if (projectileBehaviour == null)
                return;

            _sourceId = ModifierId;

            var projectileCountId = StatRegistry.GetId("projectile_count");

            projectileBehaviour.Stats.AddModifier(new StatModifier(projectileCountId, ModifierType.Flat, amount,
                _sourceId));
        }

        public void Remove(ICanBeModified target) {
            if (target is not Skill skill)
                return;

            var projectileBehaviour = skill.GetBehaviour<ProjectileBehaviour>();
            projectileBehaviour?.Stats.RemoveModifiersFromSource(_sourceId);
        }
    }
}