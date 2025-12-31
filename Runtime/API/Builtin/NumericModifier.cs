// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Spellbound.Stats {
    /// <summary>
    /// A reference implementation of IModifier that changes numerical stats.
    /// Use this as a template for creating your own numeric modifiers.
    /// </summary>
    /// <example>
    /// "Create a modifier that adds +10 to strength and has no conditional tags"
    /// var strengthMod = new NumericModifier(
    ///     modifierId: 1,
    ///     requiredTags: null, // No tag requirements
    ///     statModifier: new StatModifier(
    ///         StatRegistry.GetId("strength"),
    ///         ModifierType.Flat,
    ///         10f,
    ///         sourceId: itemId
    ///     )
    /// );
    /// 
    /// "Create a modifier that increases fire damage by 30% (conditional)"
    /// var fireMod = new NumericModifier(
    ///     modifierId: 2,
    ///     requiredTags: new HashSet&lt;int&gt; { TagRegistry.GetId("Fire") }, // Tag requirement
    ///     statModifier: new StatModifier(
    ///         StatRegistry.GetId("base_damage"),
    ///         ModifierType.Increased,
    ///         30f,
    ///         sourceId: passiveId
    ///     )
    /// );
    /// </example>
    /// <remarks>
    /// This is technically game logic, but I wanted to include it in the base package since I think that 99% of users
    /// will want this type of modifier. So please feel free to use or write your own based on your needs and use this
    /// as a reference on how to construct. 
    /// </remarks>
    public class NumericModifier : IModifier {
        public int ModifierId { get; }
        public HashSet<int> RequiredTags { get; }
        
        private readonly StatModifier _statModifier;

        public NumericModifier(int modifierId, [CanBeNull] HashSet<int> requiredTags, StatModifier statModifier) {
            ModifierId = modifierId;
            RequiredTags = requiredTags;
            _statModifier = statModifier;
        }

        public void Apply(IModifiable target) {
            // Builtin API call to check/compare tags. If RequiredTags is null then no requirements are necessary and
            // this should be able to apply.
            if (!SbModifier.ShouldModifierApply(RequiredTags, target))
                return;

            // Apply to target's stats if it has a stat container
            if (target is IStats hasStats)
                hasStats.Stats.AddModifier(_statModifier);
        }

        public void Remove(IModifiable target) {
            if (target is IStats hasStats)
                hasStats.Stats.RemoveModifiersFromSource(_statModifier.SourceId);
        }
    }
}