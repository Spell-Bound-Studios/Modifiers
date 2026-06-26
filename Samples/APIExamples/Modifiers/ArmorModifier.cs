// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample enemy modifier: adds flat <c>armor</c> to an <see cref="ArmorBehaviour"/>, cutting more physical
    /// damage off the top. Lands on a different behaviour than the resistance mods — same uniform mechanism.
    /// </summary>
    [Serializable, PackerId("sample_increased_armor")]
    public sealed class ArmorModifier : SbModifier {
        [SerializeField] private float amount = 20f;

        public override void Apply(ICanBeModified target) {
            if (TryGetBehaviour<ArmorBehaviour>(target, out var armor))
                armor.AddFlat("sample_armor", amount, UniqueId);
        }

        public override void Remove(ICanBeModified target) {
            if (TryGetBehaviour<ArmorBehaviour>(target, out var armor))
                armor.RemoveModifierByUniqueId(UniqueId);
        }

        public override void Pack(ref Span<byte> buffer) => Packer.WriteFloat(ref buffer, amount);
        public override void Unpack(ref ReadOnlySpan<byte> buffer) => amount = Packer.ReadFloat(ref buffer);
        public override ISmartPacker CreateNewInstance() => new ArmorModifier();
    }
}
