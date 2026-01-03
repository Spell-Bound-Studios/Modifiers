// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Stats {
    /// <summary>
    /// Implement this if you want something to have a stat container.
    /// Having IStats alone does NOT imply modifiability.
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