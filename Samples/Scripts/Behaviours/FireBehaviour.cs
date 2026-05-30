// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour: deals fire damage to a <see cref="TargetedPayload"/>'s target via
    /// <see cref="EnemyTarget.TakeDamage"/>, and (on a chance roll) applies an ignite that scales by the
    /// <c>increased_ignite_damage</c> stat on top of the landed hit. Stats: <c>fire_damage</c>,
    /// <c>ignite_chance</c>, <c>increased_ignite_damage</c>. Knows HOW to land a fire hit; the skill
    /// orchestrates when, against whom, and what to do with the returned <c>DamagePayload</c>.
    /// </summary>
    [Serializable]
    public sealed class FireBehaviour : SbBehaviour {
        public DamagePayload DealFireDamage(TargetedPayload payload) {
            var damage = GetValue("fire_damage");

            var enemy = payload.Target.GetComponent<EnemyTarget>();

            if (enemy != null)
                enemy.TakeDamage(damage, "fire");

            return new DamagePayload(payload.Source, payload.Target, damage, "fire", false);
        }

        public void TryIgnite(TargetedPayload payload, float duration, float hitDamage) {
            var chance = GetValue("ignite_chance");

            if (UnityEngine.Random.value * 100f >= chance)
                return;

            var increasedIgnite = GetValue("increased_ignite_damage");
            var totalIgniteDamage = hitDamage * (1f + increasedIgnite / 100f);
            var igniteDps = totalIgniteDamage / duration;

            var enemy = payload.Target.GetComponent<EnemyTarget>();
            enemy?.ApplyIgnite(duration, igniteDps);
        }
    }
}