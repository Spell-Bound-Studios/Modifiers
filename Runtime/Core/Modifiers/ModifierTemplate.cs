// Copyright 2025 Spellbound Studio Inc.

using System;

namespace Spellbound.Stats {
    /// <summary>
    /// Template for creating a single modifier with its value configuration.
    /// Pairs the value ranges (fixed/min/max) with the factory to create the modifier.
    /// </summary>
    public struct ModifierTemplate {
        /// <summary>
        /// Value to use when RollType is Fixed.
        /// </summary>
        public float FixedValue;
        
        /// <summary>
        /// Minimum value when RollType is Range.
        /// </summary>
        public float MinValue;
        
        /// <summary>
        /// Maximum value when RollType is Range.
        /// </summary>
        public float MaxValue;
        
        /// <summary>
        /// Description of what this modifier does (for preview).
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Factory function to create the modifier given a rolled value and source ID.
        /// </summary>
        public Func<float, int, IModifier> CreateModifier;
    }
}