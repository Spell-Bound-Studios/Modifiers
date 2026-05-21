// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Abstract generic base for an authored pipeline. Concrete game-side subclasses (e.g.
    /// <c>DamageReceivePipelineConfig</c>) close the generic and gain a <see cref="CreateAssetMenuAttribute"/>
    /// for asset creation. List order is execution order — drag-and-drop to reorder; there is no separate
    /// priority field.
    /// </summary>
    /// <remarks>
    /// On <see cref="Activate"/>, the inspector-authored list is baked into a non-null array
    /// (<see cref="Stages"/>) for the hot path; <see cref="Pipeline.Run{TContext}"/> iterates that array
    /// directly with no LINQ and no allocations.
    /// </remarks>
    public abstract class PipelineConfig<TContext> : ScriptableObject where TContext : struct {
        [SerializeReference] public List<IPipelineStage<TContext>> stages = new();

        public static PipelineConfig<TContext> Active { get; private set; }

        public IPipelineStage<TContext>[] Stages { get; private set; } = Array.Empty<IPipelineStage<TContext>>();

        public void Activate() {
            var baked = new List<IPipelineStage<TContext>>(stages.Count);

            foreach (var stage in stages) {
                if (stage != null)
                    baked.Add(stage);
            }

            Stages = baked.ToArray();
            Active = this;
        }
    }
}