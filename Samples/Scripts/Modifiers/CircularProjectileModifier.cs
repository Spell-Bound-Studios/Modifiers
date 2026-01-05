// Copyright 2025 Spellbound Studio Inc.

#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Modifier that changes projectile directions.
    /// </summary>
    [Serializable]
    public class CircularProjectileModifier : IModifier {
        public int ModifierId { get; }
        
        [Tooltip("Conditions that must be met")]
        public HashSet<int>? RequiredTags { get; private set; }

        // Empty constructor for editor/inspector
        public CircularProjectileModifier() { }

        // Construction
        public CircularProjectileModifier(int id, HashSet<int>? tags) {
            ModifierId = id;
            RequiredTags = tags;
        }
        
        // How to apply this modifier - comes from IModifier.
        public void Apply(ICanBeModified target) {
            // Check to see if it has matching tags with the ICanBeModified's tags.
            // In the sample scene it's checking the skills tags as the target because that's what we're applying this to.
            if (!SbModifier.ShouldModifierApply(RequiredTags, target))
                return;
            
            // Assuming they have matching tags we proceed. If you wanted to make it so that this modifier could only
            // be applied to something specific you could implement that here by simply checking the targets type.
            if (target is not Skill skill)
                return;
            
            // Then we need to define what behaviour we're going to modify. In this case we are going to change the
            // projectiles behaviour... so let's get a reference to said behaviour and change its directional cache.
            var projectileBehaviour = skill.GetBehaviour<ProjectileBehaviour>();
            
            // Simple guard to ensure we've got it.
            // Note that CalculateDirections is a func in this example, and therefore we can subscribe the Calculate
            if (projectileBehaviour != null)
                projectileBehaviour.CalculateDirections = CalculateCircularDirections;
        }
        
        // How to remove this modifier - comes from IModifier.
        public void Remove(ICanBeModified target) {
            if (target is not Skill skill)
                return;
        }
        
        private Vector3[] CalculateCircularDirections(int count) {
            if (count <= 0) 
                return Array.Empty<Vector3>();
            
            var directions = new Vector3[count];
            var angleStep = 360f / count;
            
            for (var i = 0; i < count; i++) {
                var angle = i * angleStep * Mathf.Deg2Rad;
                directions[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            }
            
            return directions;
        }
    }
}