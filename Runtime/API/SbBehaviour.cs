// Copyright 2025 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Base class for all behaviours. Pure capabilities with stats that can be modified.
    /// </summary>
    [Serializable]
    public abstract class SbBehaviour {
        private StatContainer _stats;
        public StatContainer Stats => _stats ??= InitializeStats();
        
        protected virtual StatContainer InitializeStats() => new();
    }
}