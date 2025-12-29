// Copyright 2025 Spellbound Studio Inc.

using System;

namespace Spellbound.Stats.Samples {
    public class ProjectileBehaviour : ISkillBehaviour {
        public string BehaviourType => "Projectile";
        
        public event Action<HitContext> OnHit;
        public event Action<KillContext> OnKill;
        public int ProjectileCount { get; set; }
        
        public void Execute() {
            // Fire projectiles, invoke OnHit
        }
    }
}