// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour: deals cold damage to a <see cref="TargetedPayload"/>'s target via
    /// <see cref="EnemyTarget.TakeDamage"/>, and (on a chance roll) applies a chill via
    /// <see cref="EnemyTarget.ApplyChill"/>. Stats: <c>cold_damage</c>, <c>chill_chance</c>. Knows HOW to
    /// land a cold hit; the skill orchestrates when, against whom, and what to do with the returned
    /// <c>DamagePayload</c>.
    /// </summary>
    [Serializable]
    public sealed class ColdBehaviour : SbBehaviour {
        public DamagePayload DealColdDamage(TargetedPayload payload) {
            var damage = GetValue("cold_damage");

            var enemy = payload.Target.GetComponent<EnemyTarget>();

            if (enemy != null)
                enemy.TakeDamage(damage, "cold");

            return new DamagePayload(payload.Source, payload.Target, damage, "cold", false);
        }

        public void TryChill(TargetedPayload payload, float duration) {
            var chance = GetValue("chill_chance");

            if (UnityEngine.Random.value * 100f >= chance)
                return;

            var enemy = payload.Target.GetComponent<EnemyTarget>();
            enemy?.ApplyChill(duration);
        }

    }
}
