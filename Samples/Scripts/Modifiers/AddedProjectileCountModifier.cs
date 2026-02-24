// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    [Serializable]
    public sealed class AddedProjectileCountModifier : SbModifier {
        [SerializeField] 
        private int additionalProjectiles = 6;
        
        public override void Apply(ICanBeModified target) {
            // I can use the built-in TryGetBehaviour helper to access the specific Behaviour I want out.
            if (!TryGetBehaviour<ProjectileBehaviour>(target, out var projectile))
                return;
            
            // Now I can actually add this modification to the targeted behaviours stat container.
            // In this example modifier I simply want to increase a stat value on the behaviour in an additive way.
            projectile.Stats.AddFlat("projectile_count", additionalProjectiles, UniqueId);
        }
        
        public override void Remove(ICanBeModified target) {
            // I can leverage the same try get behaviour as I did in Apply().
            if (!TryGetBehaviour<ProjectileBehaviour>(target, out var projectile))
                return;
            
            // Then I can remove this modifier by its unique ID that we have access to via the SbModifier base class.
            projectile.Stats.RemoveModifierByUniqueId(UniqueId);
        }
    }
}