// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    [Serializable]
    public sealed class IncreasedDurationModifier : SbModifier {
        [SerializeField] private float increasedDurationPercent = 50f;
        
        public override void Apply(ICanBeModified target) {
            if (target is not Skill skill) 
                return;
            
            if (!skill.Behaviours.TryGetBehaviour<DurationBehaviour>(out var duration)) 
                return;
            
            duration.Stats.AddModifier(new StatModifier(
                StatRegistry.Register("ignite_duration"),
                ModifierType.Increased,
                increasedDurationPercent,
                UniqueId
            ));
        }
        
        public override void Remove(ICanBeModified target) {
            if (target is not FireballSkill skill) 
                return;
            
            if (!skill.Behaviours.TryGetBehaviour<DurationBehaviour>(out var duration)) 
                return;
            
            duration.Stats.RemoveModifierByUniqueId(UniqueId);
        }
    }
}