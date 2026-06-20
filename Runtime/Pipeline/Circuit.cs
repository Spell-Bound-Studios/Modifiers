// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// Readable factory for a pipeline circuit: composes <see cref="PipelineNode{TContext}"/>s by Stage,
    /// Sequence, and Parallel with type inference, so an owner declares its default circuit in one expression —
    /// <c>Circuit.Sequence("root", Circuit.Stage(...), Circuit.Parallel(...), ...)</c>.
    /// </summary>
    public static class Circuit {
        public static StageNode<TContext> Stage<TContext>(string id, IPipelineStage<TContext> stage)
                where TContext : struct => new(id, stage);

        public static GroupNode<TContext> Sequence<TContext>(string id, params PipelineNode<TContext>[] children)
                where TContext : struct => new(id, PipelineGroupKind.Sequence, children);

        public static GroupNode<TContext> Parallel<TContext>(string id, params PipelineNode<TContext>[] children)
                where TContext : struct => new(id, PipelineGroupKind.Parallel, children);
    }
}
