// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using System.Globalization;

namespace Spellbound.Stats {
    /// <summary>
    /// Implement this if you want to create a modifier definition - a template that produces modifier instances.
    /// 
    /// A single definition is essentially a container for one or more IModifiers.
    /// The ModifierRollType parameter determines whether values are fixed constants or random between a range of values.
    /// 
    /// Use Cases:
    /// - Items: Use ModifierRollType.Range to get random rolls on drop
    /// - Passives: Use ModifierRollType.Fixed to get consistent max values
    /// - Crafting: Use ModifierRollType.Range with custom ranges per craft
    /// </summary>
    /// <example>
    /// // Simple: One definition creates one modifier
    /// public class StrengthModifierDef : ModifierDefinition {
    ///     public override string DisplayName => "Increased Strength";
    ///     
    ///     public override IEnumerable&lt;IModifier&gt; CreateModifiers(ModifierRollType rollType, int sourceId) {
    ///         float value = rollType == ModifierRollType.Fixed ? 15f : Random.Range(10f, 15f);
    ///         yield return new NumericModifier(
    ///             modifierId: 1, // Made up for this example
    ///             requiredTags: null, // Applies to everything
    ///             statModifier: new StatModifier(
    ///                 StatRegistry.GetId("strength"), // Assumes StatRegistry.Register("strength") was called
    ///                 ModifierType.Flat, // Flat addition
    ///                 value, // Rolled or fixed value
    ///                 sourceId // Passed in from caller
    ///             )
    ///         );
    ///     }
    ///     
    ///     public override string GetValuePreview() => "10-15 Strength";
    /// }
    /// 
    /// // Complex: One definition creates multiple modifiers
    /// public class DualDamageModifierDef : ModifierDefinition {
    ///     public override string DisplayName => "Dual Damage";
    ///     
    ///     public override IEnumerable&lt;IModifier&gt; CreateModifiers(ModifierRollType rollType, int sourceId) {
    ///         // Physical: 6-10
    ///         float physValue = rollType == ModifierRollType.Fixed ? 10f : Random.Range(6f, 10f);
    ///         yield return new NumericModifier(..., physValue, ...);
    ///         
    ///         // Fire: 6-10  
    ///         float fireValue = rollType == ModifierRollType.Fixed ? 10f : Random.Range(6f, 10f);
    ///         yield return new NumericModifier(..., fireValue, ...);
    ///     }
    ///     
    ///     public override string GetValuePreview() => "6-10 Phys, 6-10 Fire";
    /// }
    /// </example>
    public abstract class ModifierDefinition {
        /// <summary>
        /// Display name shown in the inspector dropdown.
        /// Must be unique across all modifier definitions.
        /// </summary>
        /// <example>
        /// "Increased Fire Damage", "Mana Recoup", "Dual Damage"
        /// </example>
        public abstract string DisplayName { get; }
        
        /// <summary>
        /// Get all modifier templates this definition creates.
        /// Each template contains value configuration and creation logic.
        /// </summary>
        protected abstract ModifierTemplate[] GetTemplates();

        /// <summary>
        /// Create all modifier instances for this definition.
        /// 
        /// Implementation decides:
        /// - How many modifiers to create
        /// - What values to use
        /// - What stats/behaviours to modify
        /// </summary>
        /// <param name="rollType">How to determine values - Fixed uses max, Range rolls randomly</param>
        /// <param name="sourceId">ID of what granted this (item InstanceID, passive node ID, etc.)</param>
        /// <returns>One or more modifier instances ready to be applied</returns>
        public IEnumerable<IModifier> CreateModifiers(ModifierRollType rollType, int sourceId) {
            foreach (var template in GetTemplates()) {
                var value = rollType == ModifierRollType.Fixed
                        ? template.FixedValue 
                        : UnityEngine.Random.Range(template.MinValue, template.MaxValue);
            
                yield return template.CreateModifier(value, sourceId);
            }
        }
        
        /// <summary>
        /// Get a preview string showing possible values for the inspector.
        /// Used by the property drawer to display what this modifier does.
        /// </summary>
        /// <returns>Human-readable string describing the value range(s)</returns>
        /// <example>
        /// "10-15 Strength", "6-10 Phys, 6-10 Fire", "30% increased Fire Damage"
        /// </example>
        public string GetValuePreview(ModifierRollType rollType) {
            var templates = GetTemplates();
            var previews = new List<string>();
        
            foreach (var template in templates) {
                var valueStr = rollType == ModifierRollType.Fixed
                        ? template.FixedValue.ToString(CultureInfo.InvariantCulture)
                        : $"{template.MinValue}-{template.MaxValue}";
            
                previews.Add($"{valueStr} {template.Description}");
            }
        
            return string.Join(", ", previews);
        }
    }
}