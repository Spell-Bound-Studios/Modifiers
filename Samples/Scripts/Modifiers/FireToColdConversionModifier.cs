// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    public class FireToColdConversionModifier : IModifier {
        public int ModifierId { get; }
        public HashSet<int> RequiredTags { get; }
        
        private readonly float _conversionPercent;

        public FireToColdConversionModifier(int modifierId, HashSet<int> requiredTags, float conversionPercent) {
            ModifierId = modifierId;
            RequiredTags = requiredTags;
            _conversionPercent = conversionPercent;
        }

        public void Apply(IModifiable target) {
            if (!HasMatchingTags(target))
                return;

            if (target is not Skill skill) 
                return;

            var conversion = skill.GetBehaviour<DamageConversionBehaviour>();
            if (conversion == null) {
                conversion = new DamageConversionBehaviour {
                    FromDamageType = "Fire",
                    ToDamageType = "Cold",
                    ConversionPercent = _conversionPercent
                };
                skill.AddBehaviour(conversion);
                    
                // Also add Cold tag since we now deal cold damage
                skill.Tags.Add(TagRegistry.GetId("Cold"));
            }
                
            Debug.Log($"Added {_conversionPercent}% Fire to Cold conversion to {skill.Name}");
        }

        public void Remove(IModifiable target) {
            if (target is not Skill skill) 
                return;

            skill.RemoveBehaviour<DamageConversionBehaviour>();
            skill.Tags.Remove(TagRegistry.GetId("Cold"));
        }

        private bool HasMatchingTags(IModifiable target) {
            if (RequiredTags.Count == 0)
                return true;

            foreach (var tag in RequiredTags) {
                if (target.Tags.Contains(tag))
                    return true;
            }

            return false;
        }
    }
}