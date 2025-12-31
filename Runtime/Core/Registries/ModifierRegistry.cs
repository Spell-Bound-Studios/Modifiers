// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Registry for modifier definitions.
    /// Symmetric to StatRegistry and SkillRegistry.
    /// </summary>
    public static class ModifierRegistry {
        private static readonly Dictionary<string, Type> NameToType = new();
        
        public static void Register<T>() where T : ModifierDefinition, new() {
            var instance = new T();
            NameToType[instance.DisplayName] = typeof(T);
        }
        
        public static ModifierDefinition CreateDefinition(string displayName) {
            if (NameToType.TryGetValue(displayName, out var type))
                return (ModifierDefinition)Activator.CreateInstance(type);
            
            throw new KeyNotFoundException($"Modifier '{displayName}' not registered");
        }
        
        public static IEnumerable<string> GetAllModifierNames() => NameToType.Keys;
    }
}