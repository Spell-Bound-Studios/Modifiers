// Copyright 2025 Spellbound Studio Inc.

#nullable enable
using System;
using System.Collections.Generic;
using Spellbound.Stats.Attributes;
using UnityEngine;

namespace Spellbound.Stats {
    /// <summary>
    /// A configurable numeric modifier that can be set up in the inspector or created at runtime.
    /// </summary>
    /// <example>
    /// Inspector Usage:
    /// 1. Add NumericModifier to ModifierModule
    /// 2. Configure: statId, modifierType, value, tags
    /// 3. sourceId is set automatically when item instance is created
    /// 
    /// Runtime Usage:
    /// var mod = new NumericModifier(
    ///     id: 1,
    ///     stat: StatRegistry.GetId("strength"),
    ///     type: ModifierType.Flat,
    ///     val: 10f,
    ///     tags: null,
    ///     source: itemInstanceId
    /// );
    /// </example>
    /// <remarks>
    /// We chose to include this in the library because we believe that 99% of users will just end up creating this
    /// first anyway. Please feel free to use or create your own with this as a model for numeric stat modification. 
    /// </remarks>
    [Serializable]
    public class NumericModifier : IModifier {
        [Tooltip("Unique identifier for this modifier instance")]
        [SerializeField] private int modifierId;
        
        [Tooltip("Which stat to modify")]
        [SerializeField] [StatId] private int statId;
        
        [Tooltip("How to modify")]
        [SerializeField] private ModifierType modifierType;
        
        [Tooltip("Amount to modify")]
        [SerializeField] private float value;
        
        [Tooltip("Conditions that must be met")]
        public HashSet<int>? RequiredTags { get; }

        public int ModifierId => modifierId;
        private int _sourceId;

        /// <summary>
        /// Empty constructor for Unity serialization.
        /// Call Initialize() before applying.
        /// </summary>
        public NumericModifier() { }
        
        /// <summary>
        /// Full constructor for runtime creation.
        /// </summary>
        public NumericModifier(int id, int stat, ModifierType type, float val, HashSet<int>? tags, int source) {
            modifierId = id;
            statId = stat;
            modifierType = type;
            value = val;
            RequiredTags = tags;
            _sourceId = source;
        }
        
        /// <summary>
        /// Initialize the sourceId for inspector-created modifiers.
        /// </summary>
        public void Initialize(int sourceId) => _sourceId = sourceId;
        
        public void Apply(ICanBeModified target) {
            if (!SbModifier.ShouldModifierApply(RequiredTags, target))
                return;
            
            if (target is not IStats hasStats)
                return;
            
            var statModifier = new StatModifier(statId, modifierType, value, _sourceId);
            hasStats.Stats.AddModifier(statModifier);
        }
        
        public void Remove(ICanBeModified target) {
            if (target is not IStats hasStats)
                return;
            
            hasStats.Stats.RemoveModifiersFromSource(_sourceId);
        }
    }
}