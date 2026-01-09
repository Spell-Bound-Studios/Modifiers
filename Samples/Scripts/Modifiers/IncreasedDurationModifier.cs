// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    [Serializable]
    public sealed class IncreasedDurationModifier : IModifier {
        [SerializeField] private float increasedDurationPercent = 50f;
        
        private int? _modifierId;
        public int ModifierId => _modifierId ??= GetHashCode();
        
        public void Apply(ICanBeModified target) {
            if (target is not Skill skill) return;
            if (!skill.Behaviours.TryGet<DurationBehaviour>(out var duration)) return;
            
            duration.Stats.AddModifier(new StatModifier(
                StatRegistry.Register("ignite_duration"),
                ModifierType.Increased,
                increasedDurationPercent,
                ModifierId
            ));
        }
        
        public void Remove(ICanBeModified target) {
            if (target is not FireballSkill skill) return;
            if (!skill.Behaviours.TryGet<DurationBehaviour>(out var duration)) return;
            
            duration.Stats.RemoveModifiersFromSource(ModifierId);
        }
    }
}