// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// The abstract base of a gate; answers whether the context satisfies it.
    /// </summary>
    public abstract class Condition {
        public abstract bool Met(CircuitContext ctx);
    }
}
