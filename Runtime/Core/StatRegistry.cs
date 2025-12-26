using System.Collections.Generic;

namespace Spellbound.Stats
{
    public static class StatRegistry
    {
        private static readonly Dictionary<string, int> _nameToId = new();
        private static readonly Dictionary<int, string> _idToName = new();
        private static int _nextId = 0;
        
        public static int Register(string statName)
        {
            if (_nameToId.TryGetValue(statName, out var existingId))
                return existingId;
            
            var id = _nextId++;
            _nameToId[statName] = id;
            _idToName[id] = statName;
            return id;
        }
        
        public static int GetId(string statName)
        {
            return _nameToId.TryGetValue(statName, out var id) 
                ? id 
                : throw new KeyNotFoundException($"Stat '{statName}' not registered");
        }
        
        public static bool TryGetId(string statName, out int id) 
            => _nameToId.TryGetValue(statName, out id);
        
        public static string GetName(int id) => _idToName[id];
    }
}