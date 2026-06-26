// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample modifier: a <c>more</c> multiplier on <c>fire_damage</c> for whatever owns a
    /// <see cref="FireBehaviour"/>. The lib routes it to that behaviour; the stat recomputes by the PoE rules.
    /// The canonical "+X% to a stat" shape.
    /// </summary>
    [Serializable, PackerId("sample_increased_fire_damage")]
    public sealed class IncreasedFireDamageModifier : SbModifier {
        // The lib's More/Increased take a fraction, not a percent: 1.0 = +100%, 0.5 = +50%.
        [SerializeField] private float moreFraction = 1f;

        public override void Apply(ICanBeModified target) {
            if (TryGetBehaviour<FireBehaviour>(target, out var fire))
                fire.AddMore("fire_damage", moreFraction, UniqueId);
        }

        public override void Remove(ICanBeModified target) {
            if (TryGetBehaviour<FireBehaviour>(target, out var fire))
                fire.RemoveModifierByUniqueId(UniqueId);
        }

        public override void Pack(ref Span<byte> buffer) => Packer.WriteFloat(ref buffer, moreFraction);
        public override void Unpack(ref ReadOnlySpan<byte> buffer) => moreFraction = Packer.ReadFloat(ref buffer);
        public override ISmartPacker CreateNewInstance() => new IncreasedFireDamageModifier();
    }
}
