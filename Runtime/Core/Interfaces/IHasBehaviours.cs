namespace Spellbound.Stats {
    /// <summary>
    /// Composability interface for behaviours.
    /// </summary>
    /// <example>
    /// if (target is not IHasBehaviours iBehaviours) return;
    /// </example>
    public interface IHasBehaviours {
        BehaviourContainer Behaviours { get; }
    }
}