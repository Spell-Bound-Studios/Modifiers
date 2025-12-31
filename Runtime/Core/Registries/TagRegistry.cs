// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Central registry for tag names to IDs.
    /// Tags define what modifiers apply to what (Fire, Physical, Spell, Attack, etc.)
    /// </summary>
    public static class TagRegistry {
        private static readonly Dictionary<string, int> NameToId = new();
        private static readonly Dictionary<int, string> IDToName = new();
        private static int _nextId = 0;

        /// <summary>
        /// Register a tag name and get its ID. Idempotent - safe to call multiple times.
        /// </summary>
        public static int Register(string tagName) {
            if (NameToId.TryGetValue(tagName, out var existingId))
                return existingId;

            var id = _nextId++;
            NameToId[tagName] = id;
            IDToName[id] = tagName;
            return id;
        }

        /// <summary>
        /// Get the ID for a registered tag. Throws if not registered.
        /// </summary>
        public static int GetId(string tagName) {
            return NameToId.TryGetValue(tagName, out var id) 
                    ? id 
                    : throw new KeyNotFoundException($"Tag '{tagName}' not registered");
        }

        /// <summary>
        /// Try to get the ID for a tag.
        /// </summary>
        public static bool TryGetId(string tagName, out int id) => NameToId.TryGetValue(tagName, out id);

        /// <summary>
        /// Get the name for a tag ID.
        /// </summary>
        public static string GetName(int id) => IDToName[id];
    }
}