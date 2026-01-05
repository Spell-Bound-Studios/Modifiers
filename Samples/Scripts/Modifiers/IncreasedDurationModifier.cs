// Copyright 2025 Spellbound Studio Inc.

#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Modifier that increases duration-based stats.
    /// Demonstrates a percentage-based modifier targeting Duration behaviours.
    /// </summary>
    [Serializable]
    public sealed class IncreasedDurationModifier : IModifier {
        [Tooltip("Percentage increase to all duration stats"), SerializeField]
        
        private float increasedDurationPercent = 50f;

        private int? _modifierId;

        private HashSet<int>? _requiredTags;

        private int _sourceId;
        public int ModifierId => _modifierId ??= GetHashCode();
        public HashSet<int>? RequiredTags => _requiredTags ??= new HashSet<int> { TagRegistry.Register("Duration") };

        public void Apply(ICanBeModified target) {
            if (!SbModifier.ShouldModifierApply(RequiredTags, target))
                return;

            if (target is not Skill skill)
                return;

            var durationBehaviour = skill.GetBehaviour<DurationBehaviour>();

            if (durationBehaviour == null)
                return;

            _sourceId = ModifierId;

            // Apply increased duration to all duration stats
            var baseDurationId = StatRegistry.GetId("base_duration");
            var igniteDurationId = StatRegistry.GetId("ignite_duration");

            durationBehaviour.Stats.AddModifier(new StatModifier(baseDurationId, ModifierType.Increased,
                increasedDurationPercent, _sourceId));

            durationBehaviour.Stats.AddModifier(new StatModifier(igniteDurationId, ModifierType.Increased,
                increasedDurationPercent, _sourceId));
        }

        public void Remove(ICanBeModified target) {
            if (target is not Skill skill)
                return;

            var durationBehaviour = skill.GetBehaviour<DurationBehaviour>();
            durationBehaviour?.Stats.RemoveModifiersFromSource(_sourceId);
        }
    }
}