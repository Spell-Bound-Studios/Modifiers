// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Sample implementation of Fireball skill from Path of Exile.
    /// Demonstrates how to create a skill with tags, base stats, and events.
    /// </summary>
    public class FireballSkill : Skill {
        
        // Constructor
        public FireballSkill() {
            Name = "Fireball";

            // Add behaviours that make this skill unique
            AddBehaviour(new ProjectileBehaviour { 
                ProjectileCount = 1,
                ProjectileSpeed = 104f
            });
            
            AddBehaviour(new FireBehaviour {
                CanIgnite = true,
                IgniteChance = 0.1f
            });

            // Add the appropriate tags to this skill.
            Tags.Add(TagRegistry.GetId("Fire"));
            Tags.Add(TagRegistry.GetId("Spell"));
            Tags.Add(TagRegistry.GetId("Projectile"));

            // Base stats
            Stats.SetBase(StatRegistry.GetId("base_damage"), 10f);
            Stats.SetBase(StatRegistry.GetId("cast_time"), 1.2f);
            Stats.SetBase(StatRegistry.GetId("mana_cost"), 10f);
            Stats.SetBase(StatRegistry.GetId("crit_chance"), 7f);
        }
        
        public void Cast() {
            
        }

        public float GetDamage() {
            return Stats.GetValue(StatRegistry.GetId("base_damage"));
        }
    }
}