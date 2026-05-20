// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// The single mitigation stage. Authored once in a <see cref="DamageMitigationPipelineConfig"/>; carries
    /// the full mapping table from damage stats to defensive stats with per-row strategy. Iterates every
    /// entry in the incoming delta slice and rewrites its value via the matching strategy.
    /// </summary>
    /// <remarks>
    /// Damage entries with no matching mapping pass through unchanged. The convention used here is
    /// "delta-channel values are negative for harm": the stage flips to a positive magnitude before handing
    /// to the strategy, then negates the result back into the entry.
    /// </remarks>
    [Serializable, PipelineStage(typeof(DamageMitigationContext),
         "Damage Mitigation",
         "Per-damage-type stat-based mitigation. Each row pairs a damage stat with a defensive stat and " +
         "a MitigationStrategy that does the math.")]
    public sealed class DamageMitigationStage : SbBehaviour, IPipelineStage<DamageMitigationContext> {
        [SerializeField] private List<DamageMitigation> mappings = new();

        public void Process(in DamageMitigationContext ctx) {
            var delta = ctx.Delta;

            if (delta.Entries == null || mappings == null)
                return;

            for (var i = 0; i < delta.Entries.Count; i++) {
                var entry = delta.Entries[i];

                for (var m = 0; m < mappings.Count; m++) {
                    var map = mappings[m];

                    if (map.damageStat == null || map.defensiveStat == null || map.strategy == null)
                        continue;

                    if (map.damageStat.Register() != entry.id)
                        continue;

                    var defValue = ctx.State.GetStatValue(map.defensiveStat.Register());
                    var magnitude = -entry.value;

                    if (magnitude <= 0f)
                        break;

                    var mitigated = map.strategy.Apply(magnitude, defValue);
                    entry.value = -mitigated;
                    delta.Entries[i] = entry;

                    break;
                }
            }
        }
    }
}