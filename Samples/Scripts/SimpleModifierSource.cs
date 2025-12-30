// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Simple modifier source for demonstration.
    /// In a real game, this would be items, passives, buffs, etc.
    /// </summary>
    public class SimpleModifierSource : IModifierSource {
        // This is interface bound.
        public int SourceId { get; }
        public string Description { get; }

        private readonly List<IModifier> _modifiers = new();

        public SimpleModifierSource(int sourceId, string description) {
            SourceId = sourceId;
            Description = description;
        }

        // This is example code.
        public void AddModifier(IModifier modifier) {
            _modifiers.Add(modifier);
        }

        // This is interface bound.
        public IEnumerable<IModifier> GetModifiers() => _modifiers;
    }
}