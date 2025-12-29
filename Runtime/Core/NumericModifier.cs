// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// A modifier that changes numerical stats (damage, cast speed, etc.)
    /// </summary>
    public class NumericModifier : IModifier {
        public int ModifierId { get; }
        public HashSet<int> RequiredTags { get; }
        
        private readonly StatModifier _statModifier;

        public NumericModifier(int modifierId, HashSet<int> requiredTags, StatModifier statModifier) {
            ModifierId = modifierId;
            RequiredTags = requiredTags;
            _statModifier = statModifier;
        }

        public void Apply(IModifiable target) {
            // Check if target has matching tags
            if (!HasMatchingTags(target))
                return;

            // Apply to target's stats if it has a stat container
            if (target is IStats hasStats) {
                hasStats.Stats.AddModifier(_statModifier);
            }
        }

        public void Remove(IModifiable target) {
            if (target is IStats hasStats) {
                hasStats.Stats.RemoveModifiersFromSource(_statModifier.SourceId);
            }
        }

        private bool HasMatchingTags(IModifiable target) {
            // If no required tags, applies to everything
            if (RequiredTags.Count == 0)
                return true;

            // Check for any tag overlap
            foreach (var tag in RequiredTags) {
                if (target.Tags.Contains(tag))
                    return true;
            }

            return false;
        }
    }
}