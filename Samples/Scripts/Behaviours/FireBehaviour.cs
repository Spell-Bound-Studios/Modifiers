using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    [Serializable]
    public sealed class FireBehaviour : SbBehaviour {
        [SerializeField] private float fireDamage = 20f;
        [SerializeField] private float igniteChance = 100f;
        [SerializeField] private float increasedIgniteDamage = 0f;
        
        public DamagePayload DealFireDamage(TargetedPayload payload) {
            var damage = Stats.GetValue("fire_damage");
            
            var enemy = payload.Target.GetComponent<EnemyTarget>();
            if (enemy != null)
                enemy.TakeDamage(damage, "fire");
            
            return new DamagePayload(payload.Source, payload.Target, damage, "fire", false);
        }
        
        public void TryIgnite(TargetedPayload payload, float duration, float hitDamage) {
            var chance = Stats.GetValue("ignite_chance");
    
            if (UnityEngine.Random.value * 100f >= chance)
                return;
    
            var increasedIgnite = Stats.GetValue("increased_ignite_damage");
            var totalIgniteDamage = hitDamage * (1f + increasedIgnite / 100f);
            var igniteDps = totalIgniteDamage / duration;
            
            var enemy = payload.Target.GetComponent<EnemyTarget>();
            enemy?.ApplyIgnite(duration, igniteDps);
        }
        
        protected override StatContainer InitializeStats() {
            var stats = new StatContainer();
            stats.SetBase("fire_damage", fireDamage);
            stats.SetBase("ignite_chance", igniteChance);
            stats.SetBase("increased_ignite_damage", increasedIgniteDamage);
            return stats;
        }
    }
}