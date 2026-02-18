using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    [Serializable]
    public sealed class ColdBehaviour : SbBehaviour {
        [SerializeField] private float coldDamage = 15f;
        [SerializeField] private float chillChance = 100f;
        
        public DamagePayload DealColdDamage(TargetedPayload payload) {
            var damage = Stats.GetValue("cold_damage");
            
            var enemy = payload.Target.GetComponent<EnemyTarget>();
            if (enemy != null)
                enemy.TakeDamage(damage, "cold");
            
            return new DamagePayload(payload.Source, payload.Target, damage, "cold", false);
        }
        
        public void TryChill(TargetedPayload payload, float duration) {
            var chance = Stats.GetValue("chill_chance");
            
            if (UnityEngine.Random.value * 100f >= chance)
                return;
            
            var enemy = payload.Target.GetComponent<EnemyTarget>();
            enemy?.ApplyChill(duration);
        }
        
        protected override StatContainer InitializeStats() {
            var stats = new StatContainer();
            stats.SetBase("cold_damage", coldDamage);
            stats.SetBase("chill_chance", chillChance);
            return stats;
        }
    }
}