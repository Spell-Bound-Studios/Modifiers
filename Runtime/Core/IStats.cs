// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Stats {
    /// <summary>
    /// Marker interface for things that have a stat container.
    /// </summary>
    public interface IStats {
        StatContainer Stats { get; }
    }
}