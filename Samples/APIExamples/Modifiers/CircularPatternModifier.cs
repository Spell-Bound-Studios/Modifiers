// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample modifier: swaps the direction algorithm on a <see cref="ProjectileBehaviour"/> for a 360° fan.
    /// Demonstrates that a modifier can change a behaviour's *capability* (how it picks directions), not just
    /// stat numbers — count and speed are untouched, only the geometry shifts.
    /// </summary>
    [Serializable, PackerId("sample_circular_pattern")]
    public sealed class CircularPatternModifier : SbModifier {
        private ProjectileBehaviour _behaviour;

        public override void Apply(ICanBeModified target) {
            if (!TryGetBehaviour<ProjectileBehaviour>(target, out var projectile))
                return;

            _behaviour = projectile;
            projectile.SetDirectionCalculation(Circular);
        }

        public override void Remove(ICanBeModified target) {
            _behaviour?.ClearDirectionCalculation();
            _behaviour = null;
        }

        public override void Pack(ref Span<byte> buffer) { }
        public override void Unpack(ref ReadOnlySpan<byte> buffer) { }
        public override ISmartPacker CreateNewInstance() => new CircularPatternModifier();

        private static Vector3[] Circular(int count) {
            if (count <= 0)
                return Array.Empty<Vector3>();

            var directions = new Vector3[count];
            var step = 360f / count;

            for (var i = 0; i < count; i++) {
                var angle = i * step * Mathf.Deg2Rad;
                directions[i] = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
            }

            return directions;
        }
    }
}
