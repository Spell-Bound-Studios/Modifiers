// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Stats {
    /// <summary>
    /// Base class for modifier definitions.
    /// Defines the properties and factory method for creating modifier instances.
    /// </summary>
    public abstract class ModifierDefinition {
        public abstract string DisplayName { get; }
        public abstract float FixedValue { get; }
        public abstract float MinValue { get; }
        public abstract float MaxValue { get; }
        
        /// <summary>
        /// Create an actual modifier instance with the given rolled value.
        /// </summary>
        public abstract IModifier CreateModifier(float rolledValue, int sourceId);
    }
}