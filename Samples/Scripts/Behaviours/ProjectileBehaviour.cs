// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Behaviour for projectile mechanics.
    /// </summary>
    public class ProjectileBehaviour : IBehaviour {
        public string BehaviourType => "Projectile";
        public HashSet<int> Tags { get; }
        public StatContainer Stats { get; }

        [Tooltip("Number of projectiles to fire")]
        [SerializeField] private int count = 1;

        [Tooltip("Projectile speed")]
        [SerializeField] private float speed = 10f;
        
        public Func<int, Vector3[]> CalculateDirections { get; set; }
        private readonly Func<int, Vector3[]> _originalPattern;
        
        public ProjectileBehaviour() { }
        
        public ProjectileBehaviour(Func<int, Vector3[]> initialPattern) {
            Tags = new HashSet<int> {
                TagRegistry.Register(BehaviourType)
            };
            Stats = new StatContainer();
            
            // Projectile-specific stats
            Stats.SetBase(StatRegistry.Register("projectile_count"), count);
            Stats.SetBase(StatRegistry.Register("projectile_speed"), speed);
            
            CalculateDirections = initialPattern;
            _originalPattern = initialPattern;
        }
        
        public void RestoreOriginalPattern() {
            CalculateDirections = _originalPattern;
        }
    }
}