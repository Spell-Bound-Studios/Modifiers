// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// How a <see cref="GroupNode{TContext}"/> treats its children. <see cref="Sequence"/> — order is
    /// load-bearing, each child's output feeds the next. <see cref="Parallel"/> — order is asserted not to
    /// matter; the children touch disjoint slices of the context, so a modifier can join the group without
    /// choosing a position. Both execute as one single-threaded pass; the distinction is semantic.
    /// </summary>
    public enum PipelineGroupKind {
        Sequence,
        Parallel
    }
}
