namespace Spellbound.Stats {
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