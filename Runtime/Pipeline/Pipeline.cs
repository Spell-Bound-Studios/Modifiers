// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Modifiers {
    /// <summary>
    /// A runtime, per-owner instance of an ordered <see cref="IPipelineStage{TContext}"/> sequence. An owner —
    /// a behaviour, a module, anything — holds one, gives it a starting shape (baked from a
    /// <see cref="PipelineTemplate{TContext}"/> or composed in code), then runs a context through it. Modifiers
    /// reshape this one instance — add, insert, remove — without touching any other owner's pipeline or the
    /// shared template asset.
    /// </summary>
    public sealed class Pipeline<TContext> where TContext : struct {
        private readonly List<IPipelineStage<TContext>> _stages = new();

        /// <summary>The stages in execution order. Read-only — reshape through the methods below.</summary>
        public IReadOnlyList<IPipelineStage<TContext>> Stages => _stages;

        /// <summary>Hand the context to every stage in order, top to bottom.</summary>
        public void Run(in TContext context) {
            foreach (var stage in _stages)
                stage.Process(in context);
        }

        /// <summary>Discard the current stages and take a fresh copy of these — the bake step.</summary>
        public void Bake(IEnumerable<IPipelineStage<TContext>> stages) {
            _stages.Clear();

            if (stages == null)
                return;

            foreach (var stage in stages) {
                if (stage != null)
                    _stages.Add(stage);
            }
        }

        public void Add(IPipelineStage<TContext> stage) => _stages.Add(stage);

        public void Insert(int index, IPipelineStage<TContext> stage) => _stages.Insert(index, stage);

        public bool Remove(IPipelineStage<TContext> stage) => _stages.Remove(stage);

        public void Clear() => _stages.Clear();
    }
}
