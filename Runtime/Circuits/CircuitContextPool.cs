// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.ObjectPooling;

namespace Spellbound.Modifiers {
    public sealed class CircuitContextPool : ObjectPool<CircuitContext> {
        protected override CircuitContext Create() => new();

        protected override void Reset(CircuitContext item) => item.Clear();
    }
}