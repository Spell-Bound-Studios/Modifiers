// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Stats {
    /// <summary>
    /// Serializable modifier slot for inspector usage.
    /// Stores configuration for creating modifier instances.
    /// </summary>
    [Serializable]
    public class ModifierSlot {
        public string modifierName = "";
        public RollType rollType = RollType.Fixed;
        
        /// <summary>
        /// Create a modifier instance based on this slot's configuration.
        /// </summary>
        public IModifier CreateModifier(int sourceId) {
            if (string.IsNullOrEmpty(modifierName))
                return null;
            
            var definition = ModifierRegistry.CreateDefinition(modifierName);
            
            var value = rollType switch {
                RollType.Fixed => definition.FixedValue,
                RollType.Range => UnityEngine.Random.Range(definition.MinValue, definition.MaxValue),
                _ => definition.FixedValue
            };
            
            return definition.CreateModifier(value, sourceId);
        }
    }
}