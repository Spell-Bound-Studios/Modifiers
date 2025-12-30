using System;
using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Central registry for skill types.
    /// Symmetric to StatRegistry - maps skill names to IDs and provides factory functions.
    /// </summary>
    public static class SkillRegistry {
        private static readonly Dictionary<string, int> NameToId = new();
        private static readonly Dictionary<int, string> IDToName = new();
        private static readonly Dictionary<int, Func<Skill>> Factories = new();
        private static int _nextId;

        /// <summary>
        /// Register a skill type. Name is extracted from the skill's constructor.
        /// Idempotent - safe to call multiple times.
        /// </summary>
        public static int Register<T>() where T : Skill, new() {
            var instance = new T();
            var skillName = instance.Name;

            if (NameToId.TryGetValue(skillName, out var existingId))
                return existingId;

            var id = _nextId++;
            NameToId[skillName] = id;
            IDToName[id] = skillName;
            Factories[id] = () => new T();

            return id;
        }

        /// <summary>
        /// Get the ID for a registered skill.
        /// </summary>
        public static int GetId(string skillName) {
            return NameToId.TryGetValue(skillName, out var id) 
                    ? id 
                    : throw new KeyNotFoundException($"Skill '{skillName}' not registered");
        }

        /// <summary>
        /// Try to get the ID for a skill.
        /// </summary>
        public static bool TryGetId(string skillName, out int id) => NameToId.TryGetValue(skillName, out id);

        /// <summary>
        /// Get the name for a skill ID.
        /// </summary>
        public static string GetName(int id) => IDToName[id];

        /// <summary>
        /// Create a new instance of a registered skill by ID.
        /// </summary>
        public static Skill Create(int skillId) {
            return Factories.TryGetValue(skillId, out var factory) 
                    ? factory() 
                    : throw new KeyNotFoundException($"Skill ID '{skillId}' not registered");
        }

        /// <summary>
        /// Create a new instance of a registered skill by name.
        /// </summary>
        public static Skill Create(string skillName) => Create(GetId(skillName));

        /// <summary>
        /// Get all registered skill names.
        /// </summary>
        public static IEnumerable<string> GetAllSkillNames() => NameToId.Keys;

        /// <summary>
        /// Clear all registered skills (useful for testing).
        /// </summary>
        public static void Clear() {
            NameToId.Clear();
            IDToName.Clear();
            Factories.Clear();
            _nextId = 0;
        }
    }
}