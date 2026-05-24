// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// A single stage in an ordered transformation pipeline. Concrete stages are <c>[Serializable]</c>
    /// classes implementing this interface for a specific <typeparamref name="TContext"/>; designers author
    /// them into a <see cref="PipelineTemplate{TContext}"/> asset and runtime owners bake that template into
    /// their own mutable per-instance list.
    /// </summary>
    /// <remarks>
    /// The context is passed by readonly reference so the stage cannot reassign its fields, but the context's
    /// payload/stats/pools fields hold class references — stage mutations to those propagate normally.
    /// Execution order is the order the stage appears in the owner's instance list, top to bottom.
    /// </remarks>
    public interface IPipelineStage<TContext> where TContext : struct {
        void Process(in TContext ctx);
    }
}