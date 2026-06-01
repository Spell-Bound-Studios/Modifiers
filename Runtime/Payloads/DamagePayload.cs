// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Immutable payload for damage-application events — "<see cref="Source"/> dealt <see cref="Amount"/>
    /// damage of <see cref="DamageType"/> to <see cref="Target"/>, crit was <see cref="DidCrit"/>." Emitted
    /// by damage behaviours and consumed by event subscribers for floating-number UI, on-hit modifiers, kill
    /// tracking, etc. The lib defines the SHAPE; how damage is computed and applied is the game's job.
    /// </summary>
    public readonly struct DamagePayload {
        public readonly object Source;
        public readonly GameObject Target;
        public readonly float Amount;
        public readonly string DamageType;
        public readonly bool DidCrit;

        public DamagePayload(object source, GameObject target, float amount, string damageType, bool didCrit) {
            Source = source;
            Target = target;
            Amount = amount;
            DamageType = damageType;
            DidCrit = didCrit;
        }
    }
}