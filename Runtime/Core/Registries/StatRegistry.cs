using System.Collections.Generic;

namespace Spellbound.Stats {
    public static class StatRegistry {
        private static readonly Dictionary<string, int> NameToId = new();
        private static readonly Dictionary<int, string> IdToName = new();
        private static int _nextId;

        public static int Register(string statName) {
            if (NameToId.TryGetValue(statName, out var existingId))
                return existingId;

            var id = _nextId++;
            NameToId[statName] = id;
            IdToName[id] = statName;

            return id;
        }

        public static int GetId(string statName) =>
                NameToId.TryGetValue(statName, out var id)
                        ? id
                        : throw new KeyNotFoundException($"Stat '{statName}' not registered");

        public static bool TryGetId(string statName, out int id) => NameToId.TryGetValue(statName, out id);

        public static string GetName(int id) => IdToName[id];
    }
}