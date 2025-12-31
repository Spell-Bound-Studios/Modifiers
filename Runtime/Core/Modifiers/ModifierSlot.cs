// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Serializable modifier slot for inspector configuration.
    /// Stores which modifier definition to use and how to roll its values.
    /// 
    /// Used on items, passives, buffs - anything that grants modifiers.
    /// Designer picks a modifier from dropdown and chooses Fixed or Range rolling.
    /// </summary>
    [Serializable]
    public class ModifierSlot {
        /// <summary>
        /// Name of the modifier definition.
        /// Populated by PropertyDrawer dropdown showing all registered ModifierDefinitions.
        /// </summary>
        public string modifierName = "";
        
        /// <summary>
        /// How to roll values when creating modifier instances.
        /// Fixed = use max values, Range = roll randomly between min-max.
        /// </summary>
        public ModifierRollType rollType = ModifierRollType.Fixed;
        
        /// <summary>
        /// Create all modifier instances based on this slot's configuration.
        /// Looks up the ModifierDefinition and calls CreateModifiers with the configured RollType.
        /// </summary>
        /// <param name="sourceId">ID of what's granting these modifiers (item, passive, etc.)</param>
        /// <returns>All modifiers created by the definition, ready to apply</returns>
        /// <example>
        /// foreach (var modifier in slot.CreateModifiers(item.GetInstanceID())) {
        ///     modifier.Apply(character);
        /// }
        /// </example>
        public IEnumerable<IModifier> CreateModifiers(int sourceId) {
            if (string.IsNullOrEmpty(modifierName))
                yield break; // Empty slot, nothing to create
            
            // Look up the definition by name
            var definition = ModifierRegistry.CreateDefinition(modifierName);
            
            // Create all modifiers using the configured roll type
            foreach (var modifier in definition.CreateModifiers(rollType, sourceId))
                yield return modifier;
        }
    }
}