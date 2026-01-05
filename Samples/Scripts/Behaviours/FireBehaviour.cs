// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    public class FireBehaviour : IBehaviour {
        public string BehaviourType => "Fire";
        public HashSet<int> Tags { get; }
        public StatContainer Stats { get; }
        
        [Tooltip("Base fire damage")]
        [SerializeField] private float fireDamage = 0;
        
        [Tooltip("Chance to ignite on hit (%)")]
        [SerializeField] private float igniteChance = 0;
        
        public FireBehaviour() {
            Tags = new HashSet<int> {
                TagRegistry.Register(BehaviourType)
            };
            Stats = new StatContainer();
            
            // Fire-specific stats
            Stats.SetBase(StatRegistry.Register("fire_damage"), fireDamage);
            Stats.SetBase(StatRegistry.Register("ignite_chance"), igniteChance);
        }
    }
}