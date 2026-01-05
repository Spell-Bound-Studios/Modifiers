// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Spellbound.Stats.Samples {
    public class FireBehaviour : IBehaviour, IEventAware {
        private Action<HitEvent> _onHitHandler;

        private StatContainer _stats;
        private HashSet<int> _tags;

        [Tooltip("Base fire damage"), SerializeField]
        private float fireDamage = 0;

        [Tooltip("Chance to ignite on hit (%)"), SerializeField]
        private float igniteChance = 0;

        public string BehaviourType => "Fire";
        public HashSet<int> Tags => _tags ??= new HashSet<int> { TagRegistry.Register(BehaviourType) };
        public StatContainer Stats => _stats ??= InitializeStats();

        public void Subscribe(SkillEventBus events) {
            _onHitHandler = OnHit;
            events.On(_onHitHandler);
        }

        public void Unsubscribe(SkillEventBus events) {
            if (_onHitHandler != null)
                events.Off(_onHitHandler);
        }

        private void OnHit(HitEvent evt) {
            var damage = Stats.GetValue(StatRegistry.GetId("fire_damage"));
            var chance = Stats.GetValue(StatRegistry.GetId("ignite_chance"));

            // Apply damage
            var enemy = evt.Target.GetComponent<EnemyTarget>();

            if (enemy == null)
                return;

            enemy.TakeDamage(damage, "fire");

            // Roll for ignite
            if (!(Random.value * 100f < chance))
                return;

            // Get ignite duration from DurationBehaviour
            var durationBehaviour = evt.Skill.GetBehaviour<DurationBehaviour>();
            var igniteDuration = durationBehaviour?.Stats.GetValue(StatRegistry.GetId("ignite_duration")) ?? 4f;

            enemy.ApplyIgnite(igniteDuration);
        }

        private StatContainer InitializeStats() {
            var stats = new StatContainer();
            stats.SetBase(StatRegistry.Register("fire_damage"), fireDamage);
            stats.SetBase(StatRegistry.Register("ignite_chance"), igniteChance);

            return stats;
        }
    }
}