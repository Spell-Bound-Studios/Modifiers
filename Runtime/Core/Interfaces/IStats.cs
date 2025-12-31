// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Stats {
    /// <summary>
    /// Implement this if you want something to have a stat container that can be modified.
    /// Typically used alongside IModifiable - both together enable numeric stat modifications.
    /// </summary>
    /// <example>
    /// Characters, Skills, Items
    /// </example>
    public interface IStats {
        /// <summary>
        /// The stat container holding all stats for this entity.
        /// You can register stats using StatRegistry.Register().
        /// </summary>
        StatContainer Stats { get; }
    }
}