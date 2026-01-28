// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    public sealed class SplittingProjectileModifier : SbModifier {
        [SerializeField] private int splitCount = 2;
        [SerializeField] private int splitAngle = 30;
        
        private Skill _skill;
        private Action<TargetedPayload> _handler;

        public override void Apply(ICanBeModified target) {
            if (target is not Skill skill) 
                return;
            
            if (!skill.Behaviours.TryGetBehaviour<ProjectileBehaviour>(out _)) 
                return;
            
            _skill = skill;
            _handler = SplitProjectiles;
            skill.Events.Add("hit", _handler);
        }

        public override void Remove(ICanBeModified target) {
            if (_skill == null) 
                return;
            
            _skill.Events.Remove("hit", _handler);
            _skill = null;
            _handler = null;
        }

        private void SplitProjectiles(TargetedPayload payload) {
            if (!_skill.Behaviours.TryGetBehaviour<ProjectileBehaviour>(out var projectileBehaviour)) 
                return;
            
            var hitPosition = payload.Position;
            var targetPosition = payload.Target.transform.position;
            
            // Where the projectile came from.
            var incomingDirection = (hitPosition - targetPosition).normalized;
            var outgoingDirection = -incomingDirection;
            
            var directions = CalculateSplitDirections(outgoingDirection, splitCount, splitAngle);
            
            var splitPayload = new PositionalPayload(_skill, hitPosition, Vector3.forward);
            
            var splitProjectiles = projectileBehaviour.Launch(splitPayload, directions);
            
            foreach (var projectile in splitProjectiles) {
                projectile.ExcludedTarget = payload.Target;
                
                projectile.Payload = (target, pos) => {
                    if (!_skill.Behaviours.TryGetBehaviour<FireBehaviour>(out var fire)) 
                        return;
                    
                    var targetPayload = new TargetedPayload(_skill, target, pos);
                    fire.DealDamage(targetPayload);
                    
                    if (_skill.Behaviours.TryGetBehaviour<DurationBehaviour>(out var duration))
                        fire.TryIgnite(targetPayload, duration.GetIgniteDuration());
                };
            }
        }
        
        private static Vector3[] CalculateSplitDirections(Vector3 baseDirection, int count, float angleBetween) {
            var directions = new Vector3[count];
    
            if (count == 1) {
                directions[0] = baseDirection;
                return directions;
            }
    
            // Total spread centered on baseDirection
            var totalSpread = angleBetween * (count - 1);
            var startAngle = -totalSpread / 2f;
    
            for (var i = 0; i < count; i++) {
                var currentAngle = startAngle + (i * angleBetween);
                var rotation = Quaternion.AngleAxis(currentAngle, Vector3.up);
                directions[i] = rotation * baseDirection;
            }
    
            return directions;
        }
    }
}