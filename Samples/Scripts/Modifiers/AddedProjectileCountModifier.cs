// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    [Serializable]
    public sealed class AddedProjectileCountModifier : SbModifier {
        [SerializeField] 
        private int additionalProjectiles = 6;
        
        public override void Apply(ICanBeModified target) {
            // I am attempting to target a skill specifically so I add a guard and also get the skill if applicable.
            if (target is not Skill skill) 
                return;
            
            // Now I can reference the skills behaviours and try to get the specific behaviour this modifier interacts with.
            if (!skill.Behaviours.TryGetBehaviour<ProjectileBehaviour>(out var projectile)) 
                return;
            
            // Now I can actually add this modifier to the targeted behaviour.
            // Notice that I'm constructing a StatModifier struct to encapsulate the change inside the AddModifier().
            projectile.Stats.AddModifier(new StatModifier(
                StatRegistry.Register("projectile_count"),
                ModifierType.Flat,
                additionalProjectiles,
                UniqueId // Comes from SbModifier base class.
            ));
        }
        
        public override void Remove(ICanBeModified target) {
            // Again I can guard against a specific target like a Skill (not strictly required but just showing it).
            if (target is not Skill skill) 
                return;
            
            // I can leverage the same try get behaviour as I did in the apply.
            if (!skill.Behaviours.TryGetBehaviour<ProjectileBehaviour>(out var projectile)) 
                return;
            
            // Then I can remove this modifier by its unique Id that we have access to via the SbModifier base class.
            projectile.Stats.RemoveModifierByUniqueId(UniqueId);
        }
    }
}