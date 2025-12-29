using System;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Sample implementation of Fireball skill from Path of Exile.
    /// Demonstrates how to create a skill with tags, base stats, and events.
    /// </summary>
    public class FireballSkill : Skill {
        // Your specific events that you define.
        public event Action<HitContext> OnHit;
        public event Action<KillContext> OnKill;

        public FireballSkill() {
            Name = "Fireball";

            // Register tags that determine which modifiers affect this skill
            Tags.Add(TagRegistry.GetId("Fire"));
            Tags.Add(TagRegistry.GetId("Spell"));
            Tags.Add(TagRegistry.GetId("Projectile"));
            Tags.Add(TagRegistry.GetId("AoE"));

            // Set base stats
            Stats.SetBase(StatRegistry.GetId("base_damage"), 10f);
            Stats.SetBase(StatRegistry.GetId("cast_time"), 1.2f);
            Stats.SetBase(StatRegistry.GetId("mana_cost"), 10f);
            Stats.SetBase(StatRegistry.GetId("crit_chance"), 7f);
        }

        /// <summary>
        /// Simulate casting the fireball.
        /// </summary>
        public void Cast() {
            var damage = Stats.GetValue(StatRegistry.GetId("base_damage"));
            var manaCost = Stats.GetValue(StatRegistry.GetId("mana_cost"));

            FireOnHit(new HitContext {
                Damage = damage
            });
        }

        /// <summary>
        /// Get calculated damage for display.
        /// </summary>
        public float GetDamage() {
            return Stats.GetValue(StatRegistry.GetId("base_damage"));
        }

        protected void FireOnHit(HitContext context) => OnHit?.Invoke(context);
        protected void FireOnKill(KillContext context) => OnKill?.Invoke(context);
    }
}