// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    public class AdditionalProjectilesModifier : IModifier {
        public int ModifierId { get; }
        public HashSet<int> RequiredTags { get; }
        
        private readonly int _additionalProjectiles;

        public AdditionalProjectilesModifier(int modifierId, HashSet<int> requiredTags, int additionalProjectiles) {
            ModifierId = modifierId;
            RequiredTags = requiredTags;
            _additionalProjectiles = additionalProjectiles;
        }

        public void Apply(IModifiable target) {
            if (!HasMatchingKeywords(target))
                return;

            if (target is not Skill skill || !skill.HasBehaviour<ProjectileBehaviour>()) 
                return;

            var projectile = skill.GetBehaviour<ProjectileBehaviour>();
            projectile.ProjectileCount += _additionalProjectiles;
            Debug.Log($"Added {_additionalProjectiles} projectiles to {skill.Name}. Total: {projectile.ProjectileCount}");
        }

        public void Remove(IModifiable target) {
            if (target is not Skill skill || !skill.HasBehaviour<ProjectileBehaviour>()) 
                return;

            var projectile = skill.GetBehaviour<ProjectileBehaviour>();
            projectile.ProjectileCount -= _additionalProjectiles;
        }

        private bool HasMatchingKeywords(IModifiable target) {
            if (RequiredTags.Count == 0)
                return true;

            foreach (var keyword in RequiredTags) {
                if (target.Tags.Contains(keyword))
                    return true;
            }

            return false;
        }
    }
}