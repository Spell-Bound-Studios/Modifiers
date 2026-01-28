// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    [Serializable]
    public class ProjectileBehaviour : SbBehaviour {
        [SerializeField] private int count = 1;
        [SerializeField] private float speed = 10f;
        
        public GameObject ProjectilePrefab { get; set; }
        
        private Func<int, Vector3[]> _directionOverride;
        
        public void SetDirectionCalculation(Func<int, Vector3[]> calculation) => 
            _directionOverride = calculation;
        
        public void ClearDirectionCalculation() => 
            _directionOverride = null;
        
        public List<SimpleProjectile> Launch(PositionalPayload payload, Vector3[] directions = null) {
            var spawned = new List<SimpleProjectile>();
            
            if (ProjectilePrefab == null)
                return spawned;
            
            var projectileCount = (int)Stats.GetValue(StatRegistry.GetId("projectile_count"));
            var projectileSpeed = Stats.GetValue(StatRegistry.GetId("projectile_speed"));
            
            var effectiveDirections = directions ?? CalculateDirections(projectileCount);
            
            foreach (var dir in effectiveDirections) {
                var worldDir = Quaternion.LookRotation(payload.Direction) * dir;
                var proj = UnityEngine.Object.Instantiate(ProjectilePrefab, payload.Position, Quaternion.identity);
                
                var projectile = proj.GetComponent<SimpleProjectile>();

                if (projectile == null) 
                    continue;

                projectile.Direction = worldDir;
                projectile.Speed = projectileSpeed;
                spawned.Add(projectile);
            }
            
            return spawned;
        }
        
        private Vector3[] CalculateDirections(int projectileCount) {
            if (_directionOverride != null)
                return _directionOverride(projectileCount);
            
            var directions = new Vector3[projectileCount];
            for (var i = 0; i < projectileCount; i++)
                directions[i] = Vector3.forward;
            
            return directions;
        }
        
        protected override StatContainer InitializeStats() {
            var stats = new StatContainer();
            stats.SetBase(StatRegistry.Register("projectile_count"), count);
            stats.SetBase(StatRegistry.Register("projectile_speed"), speed);
            return stats;
        }
    }
}