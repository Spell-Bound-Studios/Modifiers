// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample modifier for the *enemy* side: adds flat <c>fire_resistance</c> to a <see cref="ResistanceBehaviour"/>,
    /// so the same fireball suddenly does far less. The same <c>SbModifier</c> shape as the fireball mods, just
    /// pointed at the defender's resistance behaviour.
    /// </summary>
    [Serializable, PackerId("sample_increased_fire_resistance")]
    public sealed class FireResistanceModifier : SbModifier {
        [SerializeField] private float amount = 40f;

        public override void Apply(ICanBeModified target) {
            if (TryGetBehaviour<ResistanceBehaviour>(target, out var resistance))
                resistance.AddFlat("fire_resistance", amount, UniqueId);
        }

        public override void Remove(ICanBeModified target) {
            if (TryGetBehaviour<ResistanceBehaviour>(target, out var resistance))
                resistance.RemoveModifierByUniqueId(UniqueId);
        }

        public override void Pack(ref Span<byte> buffer) => Packer.WriteFloat(ref buffer, amount);
        public override void Unpack(ref ReadOnlySpan<byte> buffer) => amount = Packer.ReadFloat(ref buffer);
        public override ISmartPacker CreateNewInstance() => new FireResistanceModifier();
    }
}
