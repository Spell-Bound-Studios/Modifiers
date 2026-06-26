// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.ObjectPooling;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Pool of reusable <see cref="BehaviourContainer"/> shells. A consumer rents a cleared container, composes
    /// it for the instance it needs, and returns it when done — reusing shells instead of allocating one per
    /// spawn. The pool itself stays dumb: it knows how to make and clear a container, nothing about who uses it.
    /// </summary>
    public sealed class BehaviourContainerPool : ObjectPool<BehaviourContainer> {
        protected override BehaviourContainer Create() => new();

        protected override void Reset(BehaviourContainer container) => container.Clear();
    }
}
