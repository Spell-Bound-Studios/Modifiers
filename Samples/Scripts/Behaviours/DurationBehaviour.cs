// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    public class DurationBehaviour : IBehaviour {
        public string BehaviourType => "Duration";
        public HashSet<int> Tags { get; }
        public StatContainer Stats { get; }
        
        [Tooltip("Base duration for effects")]
        [SerializeField] private float baseDuration = 1f;
        
        [Tooltip("Ignite duration (seconds)")]
        [SerializeField] private float igniteDuration = 4f;
        
        public DurationBehaviour() {
            Tags = new HashSet<int> {
                TagRegistry.Register(BehaviourType)
            };
            Stats = new StatContainer();
            
            // Duration-specific stats
            Stats.SetBase(StatRegistry.Register("base_duration"), baseDuration);
            Stats.SetBase(StatRegistry.Register("ignite_duration"), igniteDuration);
        }
    }
}