// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// Composability interface for stats.
    /// </summary>
    /// <example>
    /// if (target is not IHasStats iStats) return;
    /// </example>
    public interface IHasStats {
        StatContainer Stats { get; }
    }
}