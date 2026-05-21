// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Opt-in marker: an <see cref="SbBehaviour"/> subclass tagged with this attribute appears in the picker
    /// dropdown of any <c>[DropdownPicker(typeof(PickableBehaviourAttribute))]</c>-decorated field. Without
    /// it, the subclass is excluded — keeps sample / scaffolding / lib-internal behaviours out of the
    /// designer's authoring menu so only the game's hand-crafted behaviours show.
    /// </summary>
    /// <remarks>
    /// Pair with <see cref="SerializableAttribute"/> — Unity SerializeReference also requires the type to be
    /// [Serializable], so a pickable behaviour declaration typically reads
    /// <c>[Serializable, PickableBehaviour] public sealed class MyBehaviour : SbBehaviour { ... }</c>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PickableBehaviourAttribute : Attribute { }
}
