// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Stats {
    /// <summary>
    /// Payload for positional events.
    /// </summary>
    public readonly struct PositionalPayload {
        public readonly object Source;
        public readonly Vector3 Position;
        public readonly Vector3 Direction;
        
        public PositionalPayload(object source, Vector3 position, Vector3 direction) {
            Source = source;
            Position = position;
            Direction = direction;
        }
    }
    
    public readonly struct TargetedPayload {
        public readonly object Source;
        public readonly GameObject Target;
        public readonly Vector3 Position;
        
        public TargetedPayload(object source, GameObject target, Vector3 position) {
            Source = source;
            Target = target;
            Position = position;
        }
    }
    
    /// <summary>
    /// Payload for damage events.
    /// </summary>
    public readonly struct DamagePayload {
        public readonly object Source;
        public readonly GameObject Target;
        public readonly float Amount;
        public readonly string DamageType;
        public readonly bool WasCrit;
        
        public DamagePayload(object source, GameObject target, float amount, string damageType, bool wasCrit = false) {
            Source = source;
            Target = target;
            Amount = amount;
            DamageType = damageType;
            WasCrit = wasCrit;
        }
    }
}