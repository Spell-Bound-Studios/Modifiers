// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Immutable payload for "X hit Y at position Z" events — projectile impact, beam strike, melee swing,
    /// AOE tag, etc. <see cref="Source"/> is the originator (often a skill / behaviour); <see cref="Cause"/>
    /// is the immediate proximate cause (often the projectile or beam itself), letting modifiers distinguish
    /// "I shot it" from "my split projectile shot it."
    /// </summary>
    public readonly struct TargetedPayload {
        public readonly object Source;
        public readonly GameObject Target;
        public readonly Vector3 Position;
        public readonly object Cause;

        public TargetedPayload(object source, GameObject target, Vector3 position, object cause = null) {
            Source = source;
            Target = target;
            Position = position;
            Cause = cause;
        }
    }
}