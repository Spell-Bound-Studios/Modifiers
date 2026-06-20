// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// The empty marker for anything an <see cref="IModifier"/> can target. Implementers typically also
    /// implement <see cref="IHasBehaviours"/> so a modifier has behaviours to reshape — and reaches the shared
    /// event surface through <see cref="BehaviourContainer.Events"/>.
    /// </summary>
    /// <example>
    /// Characters, items, trees, chests, terrain tiles, projectiles, transient buff carriers — anything in the
    /// game that modifiers can touch.
    /// </example>
    public interface ICanBeModified { }
}