// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample enemy modifier: adds flat <c>lightning_resistance</c> to a <see cref="ResistanceBehaviour"/> — the
    /// same shape as <see cref="FireResistanceModifier"/>, pointed at a different element.
    /// </summary>
    [Serializable, PackerId("sample_increased_lightning_resistance")]
    public sealed class LightningResistanceModifier : SbModifier {
        [SerializeField] private float amount = 40f;

        public override void Apply(ICanBeModified target) {
            if (TryGetBehaviour<ResistanceBehaviour>(target, out var resistance))
                resistance.AddFlat("lightning_resistance", amount, UniqueId);
        }

        public override void Remove(ICanBeModified target) {
            if (TryGetBehaviour<ResistanceBehaviour>(target, out var resistance))
                resistance.RemoveModifierByUniqueId(UniqueId);
        }

        public override void Pack(ref Span<byte> buffer) => Packer.WriteFloat(ref buffer, amount);
        public override void Unpack(ref ReadOnlySpan<byte> buffer) => amount = Packer.ReadFloat(ref buffer);
        public override ISmartPacker CreateNewInstance() => new LightningResistanceModifier();
    }
}
