// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// The abstract base of every element in the circuit; each one processes the context that passes through it.
    /// </summary>
    public abstract class Node {
        public abstract void Process(CircuitContext ctx);
    }
}
