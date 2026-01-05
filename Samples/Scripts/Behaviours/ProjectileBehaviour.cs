// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Behaviour for projectile mechanics.
    /// </summary>
    public class ProjectileBehaviour : IBehaviour, IEventAware {
        private Func<int, Vector3[]> _calculateDirections;

        // Store reference for unsubscribe
        private Action<CastEvent> _onCastHandler;

        private StatContainer _stats;

        private HashSet<int> _tags;

        [Tooltip("Number of projectiles to fire"), SerializeField]
        private int count = 1;

        [Tooltip("Projectile speed"), SerializeField]
        private float speed = 10f;

        public Func<int, Vector3[]> CalculateDirections {
            get => _calculateDirections ??= DefaultPattern;
            set => _calculateDirections = value;
        }

        // This gets set for the demo scene and enables the projectile spawning
        public GameObject ProjectilePrefab { get; set; }
        public Action<GameObject, Vector3> OnProjectileHit { get; set; }

        public string BehaviourType => "Projectile";
        public HashSet<int> Tags => _tags ??= new HashSet<int> { TagRegistry.Register(BehaviourType) };
        public StatContainer Stats => _stats ??= InitializeStats();

        public void Subscribe(SkillEventBus events) {
            _onCastHandler = OnCast;
            events.On(_onCastHandler);
        }

        public void Unsubscribe(SkillEventBus events) {
            if (_onCastHandler != null)
                events.Off(_onCastHandler);
        }

        private void OnCast(CastEvent evt) {
            var projectileCount = (int)Stats.GetValue(StatRegistry.GetId("projectile_count"));
            var projectileSpeed = Stats.GetValue(StatRegistry.GetId("projectile_speed"));
            var directions = CalculateDirections(projectileCount);

            Debug.Log($"[ProjectileBehaviour] Spawning {projectileCount} projectiles at speed {projectileSpeed}");

            for (var i = 0; i < directions.Length; i++) {
                var dir = directions[i];

                // Rotate direction by caster's facing
                var worldDir = Quaternion.LookRotation(evt.Direction) * dir;

                if (ProjectilePrefab != null) {
                    var proj = Object.Instantiate(ProjectilePrefab, evt.Origin, Quaternion.identity);
                    var projectile = proj.GetComponent<SimpleProjectile>();
                    projectile?.Initialize(worldDir, projectileSpeed, OnProjectileHit);
                }
                else
                    Debug.Log($"  → Projectile {i + 1}: Direction {worldDir:F2}");
            }
        }

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

        public void RestoreOriginalPattern() => _calculateDirections = null;
    }
}