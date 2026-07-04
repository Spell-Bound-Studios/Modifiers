// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// A node with exactly one child that wraps or reshapes the flow around it.
    /// </summary>
    public abstract class Decorator : Node {
        protected readonly Node Child;

        protected Decorator(Node child) => Child = child;
    }
}
