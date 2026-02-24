// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    [Serializable]
    public sealed class SplittingProjectileModifier : SbModifier {
        [SerializeField] private int splitCount = 2;
        [SerializeField] private int splitAngle = 30;
        
        private ICanBeModified _target;
        private Action<TargetedPayload> _targetedPayloadAction;
        private readonly HashSet<SimpleProjectile> _myProjectiles = new();

        public override void Apply(ICanBeModified target) {
            if (!TryGetBehaviour<ProjectileBehaviour>(target, out _)) 
                return;
            
            if (!TryGetEvents(target, out var events)) 
                return;
            
            _target = target;
            _targetedPayloadAction = SplitProjectiles;
            events.Add("hit", _targetedPayloadAction);
        }
        
        public override void Remove(ICanBeModified target) {
            if (_target == null) 
                return;
            
            if (!TryGetEvents(_target, out var events)) 
                return;
            
            events.Remove("hit", _targetedPayloadAction);
            _target = null;
            _targetedPayloadAction = null;
            _myProjectiles.Clear();
        }

        private void SplitProjectiles(TargetedPayload payload) {
            // Skip over my projectiles so that we don't recursively spawn more.
            if (payload.Cause is SimpleProjectile proj && _myProjectiles.Contains(proj)) {
                _myProjectiles.Remove(proj);
                return;
            }
        
            if (!TryGetBehaviour<ProjectileBehaviour>(_target, out var projectileBehaviour)) 
                return;

            var hitPosition = payload.Position;
            var targetPosition = payload.Target.transform.position;
        
            var incomingDirection = (hitPosition - targetPosition).normalized;
            var outgoingDirection = -incomingDirection;
        
            var directions = CalculateSplitDirections(outgoingDirection, splitCount, splitAngle);
            var splitPayload = new PositionalPayload(_target, hitPosition, Vector3.forward);
            var splitProjectiles = projectileBehaviour.Launch(splitPayload, directions);
        
            foreach (var splitProj in splitProjectiles) {
                splitProj.ExcludedTarget = payload.Target;
                _myProjectiles.Add(splitProj);
            
                splitProj.OnTargetHit = hitPayload => {
                    if (TryGetEvents(_target, out var events)) {
                        events.Invoke("hit", new TargetedPayload(
                            _target, 
                            hitPayload.Target, 
                            hitPayload.Position, 
                            hitPayload.Cause
                        ));
                    }
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