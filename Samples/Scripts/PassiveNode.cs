// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Represents a passive skill tree node that grants modifiers.
    /// </summary>
    public class PassiveNode : IModifierSource {
        public int SourceId { get; }
        public string Name { get; }

        private readonly List<IModifier> _modifiers = new();

        public PassiveNode(int sourceId, string name) {
            SourceId = sourceId;
            Name = name;
        }

        /// <summary>
        /// Add a modifier that this passive grants.
        /// </summary>
        public void AddModifier(IModifier modifier) {
            _modifiers.Add(modifier);
        }

        public IEnumerable<IModifier> GetModifiers() => _modifiers;
    }
}