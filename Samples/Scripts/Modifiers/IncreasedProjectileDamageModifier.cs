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
    public class IncreasedProjectileDamageModifier : IModifier {
        public int ModifierId { get; }
        public HashSet<int>? RequiredTags { get; private set; } = new() { TagRegistry.GetId("Projectile") };
        
        [SerializeField] private float increasePercent = 30f;

        public void Apply(ICanBeModified target) {
            if (!SbModifier.ShouldModifierApply(RequiredTags, target))
                return;
            
            if (target is not Skill skill)
                return;
            
            // Find all damage-dealing behaviors and modify their damage
            var fireBehaviour = skill.GetBehaviour<FireBehaviour>();

            fireBehaviour?.Stats.AddModifier(new StatModifier(
                StatRegistry.GetId("fire_damage"),
                ModifierType.Increased,
                increasePercent,
                ModifierId
            ));

            // Could also check for ColdBehaviour, LightningBehaviour, etc.
        }
        
        public void Remove(ICanBeModified target) {
            if (target is not Skill skill)
                return;
            
            var fireBehaviour = skill.GetBehaviour<FireBehaviour>();
            fireBehaviour?.Stats.RemoveModifiersFromSource(ModifierId);
        }
    }
}