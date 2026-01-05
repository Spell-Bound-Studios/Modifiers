// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    public class DurationBehaviour : IBehaviour {
        [Tooltip("Base duration for effects"), SerializeField]
        
        private float baseDuration = 1f;

        [Tooltip("Ignite duration (seconds)"), SerializeField]
        
        private float igniteDuration = 4f;

        private StatContainer _stats;
        private HashSet<int> _tags;

        public string BehaviourType => "Duration";
        public HashSet<int> Tags => _tags ??= new HashSet<int> { TagRegistry.Register(BehaviourType) };
        public StatContainer Stats => _stats ??= InitializeStats();

        private StatContainer InitializeStats() {
            var stats = new StatContainer();
            stats.SetBase(StatRegistry.Register("base_duration"), baseDuration);
            stats.SetBase(StatRegistry.Register("ignite_duration"), igniteDuration);

            return stats;
        }
    }
}