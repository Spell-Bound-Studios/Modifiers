// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    [Serializable]
    public sealed class CircularProjectileModifier : SbModifier {
        private ProjectileBehaviour _modifiedBehaviour;
        
        public override void Apply(ICanBeModified target) {
            if (!TryGetBehaviour<ProjectileBehaviour>(target, out var projectile)) 
                return;
            
            _modifiedBehaviour = projectile;
            projectile.SetDirectionCalculation(CalculateCircularDirections);
        }
        
        public override void Remove(ICanBeModified target) {
            _modifiedBehaviour?.ClearDirectionCalculation();
            _modifiedBehaviour = null;
        }
        
        private Vector3[] CalculateCircularDirections(int count) {
            if (count <= 0) 
                return Array.Empty<Vector3>();
            
            var directions = new Vector3[count];
            var angleStep = 360f / count;
            
            for (var i = 0; i < count; i++) {
                var angle = i * angleStep * Mathf.Deg2Rad;
                directions[i] = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
            }
            
            return directions;
        }
    }
}