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
        
        private HashSet<int> _tags;
        public HashSet<int> Tags => _tags ??= new HashSet<int> { TagRegistry.Register(BehaviourType) };
        
        private StatContainer _stats;
        public StatContainer Stats => _stats ??= InitializeStats();

        [Tooltip("Number of projectiles to fire")]
        [SerializeField] private int count = 1;

        [Tooltip("Projectile speed")]
        [SerializeField] private float speed = 10f;
        
        private Func<int, Vector3[]> _calculateDirections;
        public Func<int, Vector3[]> CalculateDirections {
            get => _calculateDirections ??= DefaultPattern;
            set => _calculateDirections = value;
        }
        private readonly Func<int, Vector3[]> _originalPattern;
        
        private StatContainer InitializeStats() {
            var stats = new StatContainer();
            stats.SetBase(StatRegistry.Register("projectile_count"), count);
            stats.SetBase(StatRegistry.Register("projectile_speed"), speed);
            return stats;
        }
        
        private static Vector3[] DefaultPattern(int projCount) {
            var directions = new Vector3[projCount];
            for (var i = 0; i < projCount; i++)
                directions[i] = Vector3.forward;
            return directions;
        }
        
        public void RestoreOriginalPattern() {
            CalculateDirections = _originalPattern;
        }
    }
}