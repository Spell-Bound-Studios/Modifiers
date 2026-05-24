// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour: spawns N projectiles from a <see cref="PositionalPayload"/>, where N comes from the
    /// <c>projectile_count</c> stat and speed from <c>projectile_speed</c>. Direction layout is pluggable via
    /// <see cref="SetDirectionCalculation"/> — that's how <see cref="CircularProjectileModifier"/> turns a
    /// straight-shot skill into a circular nova without touching the behaviour or the skill.
    /// </summary>
    [Serializable]
    public sealed class ProjectileBehaviour : SbBehaviour {
        public GameObject ProjectilePrefab { get; set; }

        private Func<int, Vector3[]> _directionOverride;

        public void SetDirectionCalculation(Func<int, Vector3[]> calculation) => _directionOverride = calculation;

        public void ClearDirectionCalculation() => _directionOverride = null;

        public List<SimpleProjectile> Launch(PositionalPayload payload, Vector3[] directions = null) {
            var spawned = new List<SimpleProjectile>();

            if (ProjectilePrefab == null)
                return spawned;

            var projectileCount = (int)this.GetValue("projectile_count");
            var projectileSpeed = this.GetValue("projectile_speed");

            Vector3[] finalDirections;

            if (directions != null)
                finalDirections = directions;
            else {
                var localDirections = CalculateDirections(projectileCount);
                finalDirections = new Vector3[localDirections.Length];

                for (var i = 0; i < localDirections.Length; i++)
                    finalDirections[i] = Quaternion.LookRotation(payload.Direction) * localDirections[i];
            }

            foreach (var dir in finalDirections) {
                var proj = UnityEngine.Object.Instantiate(ProjectilePrefab, payload.Position, Quaternion.identity);

                var projectile = proj.GetComponent<SimpleProjectile>();

                if (projectile == null)
                    continue;

                projectile.Direction = dir;
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

    }
}