// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// A node with many children that combines them.
    /// </summary>
    public abstract class Composite : Node {
        protected readonly Node[] Children;

        protected Composite(params Node[] children) {
            Children = children;
        }
    }
}