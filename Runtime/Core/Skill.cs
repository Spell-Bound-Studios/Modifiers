// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Base class for all skills.
    /// Skills have stats that can be modified and tags that determine which modifiers apply.
    /// </summary>
    public abstract class Skill : IModifiable, IHasStats {
        public string Name { get; protected set; }
        public HashSet<int> Tags { get; protected set; } = new();
        public StatContainer Stats { get; protected set; } = new();

        /// <summary>
        /// Apply all relevant modifiers from a collection to this skill.
        /// </summary>
        public void ApplyModifiers(IEnumerable<IModifier> modifiers) {
            foreach (var modifier in modifiers)
                modifier.Apply(this);
        }
    }
}