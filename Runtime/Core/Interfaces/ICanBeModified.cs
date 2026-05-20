// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// The empty marker for anything an <see cref="IModifier"/> can target. Implementers typically also
    /// implement one or more of <see cref="IHasStats"/>, <see cref="IHasBehaviours"/>, <see cref="IHasEvents"/>
    /// so modifiers have something concrete to mutate; <see cref="ModifiableObject"/> bundles all three.
    /// </summary>
    /// <example>
    /// Characters, items, trees, chests, terrain tiles, projectiles, transient buff carriers — anything in the
    /// game that modifiers can touch.
    /// </example>
    public interface ICanBeModified { }
}