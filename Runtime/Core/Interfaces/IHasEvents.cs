namespace Spellbound.Stats {
    /// <summary>
    /// Composability interface for events.
    /// </summary>
    /// <example>
    /// if (target is not IHasEvents iEvents) return;
    /// </example>
    public interface IHasEvents {
        EventContainer Events { get; }
    }
}