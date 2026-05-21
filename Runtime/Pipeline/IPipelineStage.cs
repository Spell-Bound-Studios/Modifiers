// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// A single stage in an ordered transformation pipeline. Concrete stages are <see cref="SbBehaviour"/>
    /// subclasses tagged with <see cref="PipelineStageAttribute"/>, authored into a
    /// <see cref="PipelineConfig{TContext}"/> asset.
    /// </summary>
    /// <remarks>
    /// The context is passed by readonly reference so the stage cannot reassign its fields, but the context's
    /// payload/stats/pools fields hold class references — stage mutations to those propagate normally.
    /// Execution order is the order the stage appears in the config's list, top to bottom.
    /// </remarks>
    public interface IPipelineStage<TContext> where TContext : struct {
        void Process(in TContext ctx);
    }
}