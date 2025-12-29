// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Represents a player character that aggregates modifiers from various sources.
    /// This is a sample implementation - your game might structure this differently.
    /// </summary>
    public class PlayerTemplate : IModifiable, IHasStats {
        public string Name { get; set; }
        public HashSet<int> Tags { get; private set; }
        public StatContainer Stats { get; private set; }

        private readonly List<IModifierSource> _modifierSources = new();

        public PlayerTemplate(string name) {
            Name = name;
            Tags = new HashSet<int>();
            Stats = new StatContainer();
        }

        /// <summary>
        /// Add a modifier source (passive node, item, buff, etc.)
        /// </summary>
        public void AddModifierSource(IModifierSource source) {
            _modifierSources.Add(source);
        }

        /// <summary>
        /// Remove a modifier source by its ID.
        /// </summary>
        public void RemoveModifierSource(int sourceId) {
            _modifierSources.RemoveAll(s => s.SourceId == sourceId);
        }

        /// <summary>
        /// Get all modifiers from all sources.
        /// Skills will query this to get relevant modifiers.
        /// </summary>
        public IEnumerable<IModifier> GetAllModifiers() {
            foreach (var source in _modifierSources) {
                foreach (var modifier in source.GetModifiers())
                    yield return modifier;
            }
        }
    }
}