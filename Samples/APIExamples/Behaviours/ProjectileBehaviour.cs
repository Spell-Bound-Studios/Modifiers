// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour: the projectile-delivery capability of a skill. Owns <c>projectile_count</c> and
    /// <c>projectile_speed</c>, and produces the world-space directions the skill fires along. The pattern is
    /// pluggable via <see cref="SetDirectionCalculation"/> — that's how <see cref="CircularPatternModifier"/>
    /// turns a forward shot into a 360° nova without touching this behaviour or the skill.
    /// </summary>
    [Serializable]
    public sealed class ProjectileBehaviour : SbBehaviour {
        private static uint? _countHash;
        private static uint? _speedHash;

        private static uint ProjectileCountHash => _countHash ??= StatRegistry.GetHash("sample_projectile_count");
        private static uint ProjectileSpeedHash => _speedHash ??= StatRegistry.GetHash("sample_projectile_speed");

        private Func<int, Vector3[]> _directionOverride;

        public bool SplitOnHit { get; set; }

        public override IReadOnlyList<StatAndValue> DeclareOwnedStats() => new[] {
            OwnedStat("sample_projectile_count", 1f),
            OwnedStat("sample_projectile_speed", 14f)
        };

        public int Count => Mathf.Max(1, (int)GetValue(ProjectileCountHash));
        public float Speed => GetValue(ProjectileSpeedHash);

        public void SetDirectionCalculation(Func<int, Vector3[]> calculation) => _directionOverride = calculation;
        public void ClearDirectionCalculation() => _directionOverride = null;

        /// <summary>The launch directions in world space, rotated to face <paramref name="forward"/>.</summary>
        public Vector3[] WorldDirections(Vector3 forward, int count) {
            var local = _directionOverride != null ? _directionOverride(count) : ForwardSpread(count);
            var rotation = Quaternion.LookRotation(forward == Vector3.zero ? Vector3.forward : forward);
            var world = new Vector3[local.Length];

            for (var i = 0; i < local.Length; i++)
                world[i] = rotation * local[i];

            return world;
        }

        private static Vector3[] ForwardSpread(int count) {
            var directions = new Vector3[count];

            if (count == 1) {
                directions[0] = Vector3.forward;

                return directions;
            }

            const float spread = 15f;
            var start = -spread * (count - 1) / 2f;

            for (var i = 0; i < count; i++)
                directions[i] = Quaternion.AngleAxis(start + i * spread, Vector3.up) * Vector3.forward;

            return directions;
        }
    }
}
