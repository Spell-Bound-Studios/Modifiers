// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Single entry point for running any ordered transformation pipeline. The active
    /// <see cref="PipelineConfig{TContext}"/> for the requested context provides the stage list; the runner
    /// iterates the pre-baked array in a tight loop with no LINQ and no per-event allocations.
    /// </summary>
    public static class Pipeline {
        // Each missing-config context type logs exactly once so a misconfigured boot is visible without
        // spamming the console on every event in the hot path.
        private static readonly HashSet<Type> WarnedMissingConfig = new();

        public static void Run<TContext>(in TContext ctx) where TContext : struct {
            var config = PipelineConfig<TContext>.Active;

            if (config == null) {
                var t = typeof(TContext);

                if (WarnedMissingConfig.Add(t)) {
                    Log.Error(
                        $"[Pipeline] No active PipelineConfig<{t.Name}>. Events for this context will not run. " +
                        "Add a PipelineConfigLoader to your boot scene and assign (or Resources-load) the matching " +
                        "config asset.");
                }

                return;
            }

            var stages = config.Stages;

            foreach (var t in stages)
                t.Process(in ctx);
        }
    }
}