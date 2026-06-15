// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample modifier: replaces the projectile direction calculation on a target's
    /// <see cref="ProjectileBehaviour"/> with a 360° fan. Demonstrates that a modifier can change BEHAVIOUR
    /// (the algorithm for picking directions), not just stat numbers — the projectile count and speed are
    /// untouched; only the geometry shifts.
    /// </summary>
    [Serializable, PackerId("circular_projectile")]
    public sealed class CircularProjectileModifier : SbModifier {
        private ProjectileBehaviour _modifiedBehaviour;

        public override void Apply(ICanBeModified target) {
            if (!TryGetBehaviour<ProjectileBehaviour>(target, out var projectile))
                return;

            _modifiedBehaviour = projectile;
            projectile.SetDirectionCalculation(CalculateCircularDirections);
        }

        public override void Remove(ICanBeModified target) {
            _modifiedBehaviour?.ClearDirectionCalculation();
            _modifiedBehaviour = null;
        }

        // No serialized state — the behaviour reference is set in Apply and the direction
        // calculation is hardcoded. Empty Pack/Unpack; Activator-built clones get a default
        // instance with the same algorithm.
        public override void Pack(ref Span<byte> buffer) { }
        public override void Unpack(ref ReadOnlySpan<byte> buffer) { }

        public override ISmartPacker CreateNewInstance() => new CircularProjectileModifier();

        private Vector3[] CalculateCircularDirections(int count) {
            if (count <= 0)
                return Array.Empty<Vector3>();

            var directions = new Vector3[count];
            var angleStep = 360f / count;

            for (var i = 0; i < count; i++) {
                var angle = i * angleStep * Mathf.Deg2Rad;
                directions[i] = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
            }

            return directions;
        }
    }
}