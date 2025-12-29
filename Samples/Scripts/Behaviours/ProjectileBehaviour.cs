// Copyright 2025 Spellbound Studio Inc.

using System;

namespace Spellbound.Stats.Samples {
    public class ProjectileBehaviour : IBehaviour {
        // Tag Name
        public string BehaviourType => "Projectile";
        
        // Delegate/Event Types
        public event Action<HitContext> OnHit;
        public event Action<KillContext> OnKill;
        
        // Implicit Behaviours
        public int ProjectileCount { get; set; } = 1;
        public float ProjectileSpeed { get; set; } = 100f;
        
        public void Execute() {
            
        }
    }
}