/*// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

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

        public float GetDamage() => Stats.GetValue(StatRegistry.GetId("base_damage"));

        public override void Execute() {
            var damage = GetDamage();
            Debug.Log($"Fireball will deal {damage:F2} damage!");

            if (!HasBehaviour<ProjectileBehaviour>()) 
                return;

            var proj = GetBehaviour<ProjectileBehaviour>();
            Debug.Log($"Firing {proj.ProjectileCount} projectiles at {proj.ProjectileSpeed} speed");
        }
    }
}*/