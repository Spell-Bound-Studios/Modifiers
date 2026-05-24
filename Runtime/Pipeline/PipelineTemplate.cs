// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored list of <see cref="IPipelineStage{TContext}"/> entries used as the vanilla starting
    /// shape for per-instance pipelines. Consumers create one ScriptableObject per pipeline kind (e.g. a
    /// damage pipeline template), drop stages into the list in execution order, and at runtime each owner
    /// (a behaviour, a module, anything) bakes its own mutable copy of the references via whatever bake
    /// method it exposes.
    /// </summary>
    /// <remarks>
    /// There is no global Active singleton — modifiers customise an owner's instance list (insert, remove,
    /// reorder) without affecting any other owner. Stages themselves are expected to be stateless: per-row
    /// configuration is constructor-baked, and a modifier wanting different behaviour swaps the stage rather
    /// than mutating it in place.
    /// </remarks>
    public abstract class PipelineTemplate<TContext> : ScriptableObject where TContext : struct {
        [SerializeReference, DropdownPicker]
        public List<IPipelineStage<TContext>> stages = new();
    }
}
