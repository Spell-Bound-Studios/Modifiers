// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers {
    [Serializable]
    [SerializeReferenceLabel("Fixed value")]
    public sealed class FixedMagnitude : ScalarMagnitude {
        [SerializeField, Tooltip("A constant amount. Never rolls.")]
        private float value;

        public override float Value(float baked) => value;
    }
}
