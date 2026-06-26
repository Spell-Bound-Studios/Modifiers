// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// One node in a pipeline circuit — a <see cref="StageNode{TContext}"/> leaf that runs an
    /// <see cref="IPipelineStage{TContext}"/>, or a <see cref="GroupNode{TContext}"/> that runs child nodes in
    /// Sequence or Parallel. Every node carries a stable <see cref="Id"/> so a modifier can target it by name —
    /// find it, insert before it, replace it, remove it. <see cref="Process"/> hands the context down the subtree.
    /// </summary>
    public abstract class PipelineNode<TContext> where TContext : struct {
        public string Id { get; }

        protected PipelineNode(string id) => Id = id;

        public abstract void Process(in TContext context);

        /// <summary>This node when its id matches; groups override to search their children too.</summary>
        public virtual PipelineNode<TContext> Find(string id) => Id == id ? this : null;
    }
}
