// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// Composability contract: "this target owns a <see cref="StatContainer"/>." The most fundamental of the
    /// three composability contracts — anything with stats can have its numbers manipulated by modifiers via
    /// <c>stats.AddFlat / AddIncreased / AddMore</c> through <see cref="ContainerExtensions"/>.
    /// </summary>
    /// <example>
    /// if (target is not IHasStats iStats) return;
    /// </example>
    public interface IHasStats {
        StatContainer Stats { get; }
    }
}