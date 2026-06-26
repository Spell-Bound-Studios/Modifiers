// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample enemy modifier: equips fire reflect by *inserting* a <see cref="ReflectStage"/> at the front of the
    /// defender's circuit — so you watch the node appear in the circuit view when you equip it. Removing it pulls
    /// the stage back out. This is circuit modification, not a stat change.
    /// </summary>
    [Serializable, PackerId("sample_reflect_fire")]
    public sealed class ReflectFireModifier : SbModifier {
        [SerializeField] private float fraction = 0.25f;

        public override void Apply(ICanBeModified target) {
            if (TryGetBehaviour<PipelineBehaviour>(target, out var pipeline)
                    && pipeline.Root is GroupNode<DamageContext> sequence) {
                sequence.Prepend(Circuit.Stage("reflect-fire", new ReflectStage("sample_fire_damage", fraction)));
                Log.Info($"[Reflect] inserted reflect-fire (x{fraction}) — circuit root now has {sequence.Children.Count} children");
            }
            else {
                Log.Warn($"[Reflect] Apply FAILED on {target?.GetType().Name} — no PipelineBehaviour, or root is not a sequence");
            }
        }

        public override void Remove(ICanBeModified target) {
            if (TryGetBehaviour<PipelineBehaviour>(target, out var pipeline)
                    && pipeline.Root is GroupNode<DamageContext> sequence)
                sequence.Remove("reflect-fire");
        }

        public override void Pack(ref Span<byte> buffer) => Packer.WriteFloat(ref buffer, fraction);
        public override void Unpack(ref ReadOnlySpan<byte> buffer) => fraction = Packer.ReadFloat(ref buffer);
        public override ISmartPacker CreateNewInstance() => new ReflectFireModifier();
    }
}
