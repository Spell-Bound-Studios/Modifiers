// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Central registry for skill types.
    /// Allows skills to be registered and instantiated by name.
    /// </summary>
    public static class SkillRegistry {
        private static readonly Dictionary<string, Type> SkillTypes = new();
    
        /// <summary>
        /// Register a skill type by name.
        /// </summary>
        public static void Register<T>(string skillName) where T : Skill, new() {
            if (SkillTypes.ContainsKey(skillName)) {
                UnityEngine.Debug.LogWarning($"Skill '{skillName}' is already registered. Overwriting.");
            }
            
            SkillTypes[skillName] = typeof(T);
        }
    
        /// <summary>
        /// Create a skill instance without specifying the type.
        /// </summary>
        public static Skill Create(string skillName) {
            if (SkillTypes.TryGetValue(skillName, out var type)) {
                return (Skill)Activator.CreateInstance(type);
            }
            
            throw new KeyNotFoundException($"Skill '{skillName}' not registered");
        }

        /// <summary>
        /// Check if a skill is registered.
        /// </summary>
        public static bool IsRegistered(string skillName) {
            return SkillTypes.ContainsKey(skillName);
        }

        /// <summary>
        /// Get all registered skill names.
        /// </summary>
        public static IEnumerable<string> GetAllSkillNames() => SkillTypes.Keys;

        /// <summary>
        /// Clear all registered skills (useful for testing).
        /// </summary>
        public static void Clear() {
            SkillTypes.Clear();
        }
    }
}