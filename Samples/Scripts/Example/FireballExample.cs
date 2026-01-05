// Copyright 2025 Spellbound Studio Inc.

using Spellbound.Core;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    public class FireballTest : MonoBehaviour {
        [SerializeField] private ObjectPreset fireballSkill;
        
        private void Start() {
            if (!fireballSkill.TryGetModule<SkillModule>(out var skillModule)) {
                Debug.LogError("No SkillModule found on Fireball preset!");
                return;
            }
            
            var fireball = skillModule.CreateSkill();
            
            Debug.Log("=== BEFORE MODIFIER ===");
            LogSkillState(fireball);
            
            var circularMod = new CircularProjectileModifier();
            circularMod.Apply(fireball);
            
            Debug.Log("=== AFTER MODIFIER ===");
            LogSkillState(fireball);
        }
        
        private static void LogSkillState(Skill skill) {
            var projectileBehaviour = skill.GetBehaviour<ProjectileBehaviour>();
            if (projectileBehaviour == null) {
                Debug.LogWarning("No ProjectileBehaviour found!");
                return;
            }
            
            var count = (int)projectileBehaviour.Stats.GetValue(StatRegistry.GetId("projectile_count"));
            var directions = projectileBehaviour.CalculateDirections(count);
            
            Debug.Log($"Projectile Count: {count}");
            Debug.Log($"Directions: {string.Join(", ", directions)}");
        }
    }
}