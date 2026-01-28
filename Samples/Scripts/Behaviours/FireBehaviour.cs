// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    [Serializable]
    public class FireBehaviour : SbBehaviour {
        [SerializeField] private float fireDamage = 50f;
        [SerializeField] private float igniteChance = 50f;
        
        public DamagePayload DealDamage(TargetedPayload payload) {
            var damage = Stats.GetValue(StatRegistry.GetId("fire_damage"));
            
            var enemy = payload.Target.GetComponent<EnemyTarget>();
            if (enemy != null)
                enemy.TakeDamage(damage, "fire");
            
            return new DamagePayload(payload.Source, payload.Target, damage, "fire", false);
        }
        
        public void TryIgnite(TargetedPayload payload, float duration) {
            var chance = Stats.GetValue(StatRegistry.GetId("ignite_chance"));
            
            if (UnityEngine.Random.value * 100f >= chance)
                return;
            
            var enemy = payload.Target.GetComponent<EnemyTarget>();
            enemy?.ApplyIgnite(duration);
        }
        
        protected override StatContainer InitializeStats() {
            var stats = new StatContainer();
            stats.SetBase(StatRegistry.Register("fire_damage"), fireDamage);
            stats.SetBase(StatRegistry.Register("ignite_chance"), igniteChance);
            return stats;
        }
    }
}