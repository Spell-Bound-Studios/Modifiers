// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Concrete skill: Fireball.
    /// </summary>
    public class FireballSkill : Skill {
        public override string Name => "Fireball";
        
        public GameObject ProjectilePrefab { get; set; }
        
        public FireballSkill() {
            Behaviours.Add(new ProjectileBehaviour());
            Behaviours.Add(new FireBehaviour());
            Behaviours.Add(new DurationBehaviour());
        }
        
        public override void Initialize() {
            Events.Register<PositionalPayload>("cast");
            Events.Register<TargetedPayload>("hit");
            Events.Register<DamagePayload>("damage");
            
            var projectile = Behaviours.Get<ProjectileBehaviour>();
            var fire = Behaviours.Get<FireBehaviour>();
            var duration = Behaviours.Get<DurationBehaviour>();
            
            projectile.ProjectilePrefab = ProjectilePrefab;
            
            Events.Add<PositionalPayload>("cast", payload => {
                var projectiles = projectile.Launch(payload);
                foreach (var p in projectiles) {
                    p.Payload = (target, pos) => 
                            Events.Invoke("hit", new TargetedPayload(this, target, pos));
                }
            });
            
            Events.Add<TargetedPayload>("hit", payload => {
                var damageResult = fire.DealDamage(payload);
                Events.Invoke("damage", damageResult);
                
                var igniteDuration = duration.GetIgniteDuration();
                fire.TryIgnite(payload, igniteDuration);
            });
        }
        
        public void Cast(Vector3 position, Vector3 direction) =>
                Events.Invoke("cast", new PositionalPayload(this, position, direction));
    }
}