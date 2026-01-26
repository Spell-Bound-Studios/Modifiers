// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    [Serializable]
    public sealed class AddedProjectileCountModifier : IModifier {
        [SerializeField] private int additionalProjectiles = 6;
        
        private int? _modifierId;
        public int ModifierId => _modifierId ??= GetHashCode();
        
        public void Apply(ICanBeModified target) {
            if (target is not Skill skill) 
                return;
            
            if (!skill.Behaviours.TryGetBehaviour<ProjectileBehaviour>(out var projectile)) 
                return;
            
            projectile.Stats.AddModifier(new StatModifier(
                StatRegistry.Register("projectile_count"),
                ModifierType.Flat,
                additionalProjectiles,
                ModifierId
            ));
        }
        
        public void Remove(ICanBeModified target) {
            if (target is not FireballSkill skill) 
                return;
            
            if (!skill.Behaviours.TryGetBehaviour<ProjectileBehaviour>(out var projectile)) 
                return;
            
            projectile.Stats.RemoveModifiersFromSource(ModifierId);
        }
    }
}