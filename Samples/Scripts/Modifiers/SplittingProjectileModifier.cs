// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    public sealed class SplittingProjectileModifier : SbModifier {
        [SerializeField] private int splitCount = 2;
        [SerializeField] private int splitAngle = 30;
        
        private ICanBeModified _target;
        private Action<TargetedPayload> _handler;

        public override void Apply(ICanBeModified target) {
            if (!TryGetBehaviour<ProjectileBehaviour>(target, out _)) 
                return;
            
            if (!TryGetEvents(target, out var events)) 
                return;
            
            _target = target;
            _handler = SplitProjectiles;
            events.Add("hit", _handler);
        }
        
        public override void Remove(ICanBeModified target) {
            if (_target == null) 
                return;
            
            if (!TryGetEvents(_target, out var events)) 
                return;
            
            events.Remove("hit", _handler);
            _target = null;
            _handler = null;
        }

        private void SplitProjectiles(TargetedPayload payload) {
            if (!TryGetBehaviour<ProjectileBehaviour>(_target, out var projectileBehaviour)) 
                return;

            var hitPosition = payload.Position;
            var targetPosition = payload.Target.transform.position;
            
            // Where the projectile came from.
            var incomingDirection = (hitPosition - targetPosition).normalized;
            var outgoingDirection = -incomingDirection;
            
            var directions = CalculateSplitDirections(outgoingDirection, splitCount, splitAngle);
            
            var splitPayload = new PositionalPayload(_target, hitPosition, Vector3.forward);
            
            var splitProjectiles = projectileBehaviour.Launch(splitPayload, directions);
            
            foreach (var proj in splitProjectiles) {
                proj.Payload = (hitTarget, pos) => {
                    if (!TryGetBehaviour<FireBehaviour>(_target, out var fire)) 
                        return;
                    
                    var targetPayload = new TargetedPayload(_target, hitTarget, pos);
                    fire.DealDamage(targetPayload);
                    
                    if (TryGetBehaviour<DurationBehaviour>(_target, out var duration))
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