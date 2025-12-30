// Copyright 2025 Spellbound Studio Inc.

using System;

namespace Spellbound.Stats.Samples {
    public class ProjectileBehaviour : IBehaviour {
        // Tag Name
        public string BehaviourType => "Projectile";
        
        // Delegate/Event Types
        public event Action<object> OnHit;
        public event Action<object> OnKill;
        
        // Implicit Behaviours
        public int ProjectileCount { get; set; } = 1;
        public float ProjectileSpeed { get; set; } = 100f;
        
        public void TriggerHit(object target) => OnHit?.Invoke(target);
        public void TriggerKill(object target) => OnKill?.Invoke(target);
    }
}