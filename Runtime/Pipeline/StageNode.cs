// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// A leaf <see cref="PipelineNode{TContext}"/>: runs one <see cref="IPipelineStage{TContext}"/> when the
    /// current reaches it.
    /// </summary>
    public sealed class StageNode<TContext> : PipelineNode<TContext> where TContext : struct {
        private readonly IPipelineStage<TContext> _stage;

        public StageNode(string id, IPipelineStage<TContext> stage) : base(id) => _stage = stage;

        public override void Process(in TContext context) => _stage.Process(in context);
    }
}
