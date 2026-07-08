// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers {
    [Serializable]
    [SerializeReferenceLabel("Random roll")]
    public sealed class RolledMagnitude : ScalarMagnitude {
        [SerializeField, Tooltip("Lowest possible rolled amount.")]
        private float min;

        [SerializeField, Tooltip("Highest possible rolled amount.")]
        private float max;

        [SerializeField, Tooltip("Snap the roll to multiples of this. 1 = whole numbers, 0.5 = halves, 0 = any value.")]
        private float step;

        public override bool Rolls => true;

        public override float Bake(System.Random rng) {
            var rolled = min + (float)rng.NextDouble() * (max - min);

            if (step > 0f)
                rolled = min + Mathf.Round((rolled - min) / step) * step;

            return Mathf.Clamp(rolled, min, max);
        }

        public override float Value(float baked) => baked;
    }
}
