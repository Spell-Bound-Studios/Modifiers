using UnityEngine;

namespace Spellbound.Stats.Samples {
    public class FireballSkill : Skill {
        public override string Name => "Fireball";
        
        public GameObject ProjectilePrefab { get; set; }
        
        // Cache behaviour references
        private ProjectileBehaviour _projectile;
        private FireBehaviour _fire;
        private DurationBehaviour _duration;
        
        public FireballSkill() {
            Behaviours.Add(new ProjectileBehaviour());
            Behaviours.Add(new FireBehaviour());
            Behaviours.Add(new DurationBehaviour());
        }
        
        public override void Initialize() {
            _projectile = Behaviours.GetBehaviour<ProjectileBehaviour>();
            _fire = Behaviours.GetBehaviour<FireBehaviour>();
            _duration = Behaviours.GetBehaviour<DurationBehaviour>();
            
            _projectile.ProjectilePrefab = ProjectilePrefab;
            
            // Attaching specific payload types to specific events and then passing in those methods to be called
            // when the event fires.
            Events.Add<PositionalPayload>("cast", OnCast);
            Events.Add<TargetedPayload>("hit", OnHit);
        }
        
        public void Cast(Vector3 position, Vector3 direction) =>
            Events.Invoke("cast", new PositionalPayload(this, position, direction));
        
        private void OnCast(PositionalPayload payload) {
            var projectiles = _projectile.Launch(payload);
            
            foreach (var proj in projectiles) {
                proj.Payload = TriggerHitEvent;
            }
        }
        
        private void TriggerHitEvent(GameObject target, Vector3 position) {
            Events.Invoke("hit", new TargetedPayload(this, target, position));
        }
        
        private void OnHit(TargetedPayload payload) {
            var damageResult = _fire.DealDamage(payload);
            Events.Invoke("damage", damageResult);
            
            _fire.TryIgnite(payload, _duration.GetIgniteDuration());
        }
    }
}