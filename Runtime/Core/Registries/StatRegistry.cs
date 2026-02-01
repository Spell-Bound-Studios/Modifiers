using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats {
    public static class StatRegistry {
        private static readonly Dictionary<string, int> NameToId = new();
        private static readonly Dictionary<int, string> IdToName = new();
        private static int _nextId;
        
        private static HashSet<string> _databaseStats;
        
        public static bool StrictValidationEnabled { get; private set; }
        
        /// <summary>
        /// Enables strict validation. Any stat not in the provided set will throw an exception.
        /// </summary>
        public static void EnableStrictValidation(IEnumerable<string> databaseStats) {
            StrictValidationEnabled = true;
            _databaseStats = new HashSet<string>(databaseStats);
        }
        
        /// <summary>
        /// Disables strict validation. Stats can be registered from anywhere.
        /// </summary>
        public static void DisableStrictValidation() {
            StrictValidationEnabled = false;
            _databaseStats = null;
        }
        
        /// <summary>
        /// Clears all registered stats. Useful for tests.
        /// </summary>
        public static void Clear() {
            NameToId.Clear();
            IdToName.Clear();
            _nextId = 0;
            StrictValidationEnabled = false;
            _databaseStats = null;
        }

        public static int Register(string statName) {
            if (NameToId.TryGetValue(statName, out var existingId))
                return existingId;
            
            if (StrictValidationEnabled && !_databaseStats.Contains(statName)) {
                throw new KeyNotFoundException(
                    $"Stat '{statName}' is not defined in StatDatabase. " +
                     "Add it to your database or disable strict validation.");
            }

            var id = _nextId++;
            NameToId[statName] = id;
            IdToName[id] = statName;

            return id;
        }

        public static int GetId(string statName) =>
            NameToId.TryGetValue(statName, out var id)
                ? id
                : throw new KeyNotFoundException($"Stat '{statName}' not registered");

        public static bool TryGetId(string statName, out int id) => 
            NameToId.TryGetValue(statName, out id);

        public static string GetName(int id) => IdToName[id];
        
        public static bool IsRegistered(string statName) => NameToId.ContainsKey(statName);
        
        public static IEnumerable<string> GetAllStatNames() => NameToId.Keys;
    }
}