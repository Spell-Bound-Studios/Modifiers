// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour: owns the damage CIRCUIT — the series/parallel tree the incoming current traverses — and
    /// runs a typed-damage list through it. Owns no stats; armor / resistances / shield live on their own
    /// behaviours and stages read them by name off the context. The default circuit is
    /// <c>absorption -> resistances(parallel) -> armor -> life</c>; a modifier reshapes this one instance by
    /// finding a node by id and inserting / replacing / removing.
    /// </summary>
    [Serializable]
    public sealed class PipelineBehaviour : SbBehaviour {
        private PipelineNode<DamageContext> _circuit;

        /// <summary>The circuit root, built on first use. Find a node by id to rearrange it.</summary>
        public PipelineNode<DamageContext> Root => _circuit ??= BuildDefaultCircuit();

        /// <summary>
        /// Run an incoming typed-damage list through the circuit. Stages read the defender's stats off
        /// <paramref name="defender"/>; the terminal stage deposits the survivors into the life pool.
        /// </summary>
        public void Mitigate(List<StatAndValue> damage, BehaviourContainer defender, PlayerController attacker = null) {
            var ctx = new DamageContext {
                Incoming = damage,
                Defender = defender,
                Attacker = attacker
            };

            Root.Process(in ctx);
        }

        private static PipelineNode<DamageContext> BuildDefaultCircuit() =>
                Circuit.Sequence("root",
                    Circuit.Stage("absorption", new AbsorptionStage()),
                    Circuit.Parallel("resistances",
                        Circuit.Stage("fire", new ResistanceStage("sample_fire_resistance", "sample_fire_damage")),
                        Circuit.Stage("cold", new ResistanceStage("sample_cold_resistance", "sample_cold_damage")),
                        Circuit.Stage("lightning", new ResistanceStage("sample_lightning_resistance", "sample_lightning_damage"))),
                    Circuit.Stage("armor", new ArmorStage()),
                    Circuit.Stage("deposit", new DepositToLifeStage()));
    }
}
