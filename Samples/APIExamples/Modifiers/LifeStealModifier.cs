// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample fireball modifier: switches on life steal — the caster heals for a fraction of the life damage each
    /// hit actually dealt. A capability toggle on the attacker's <see cref="LifeStealBehaviour"/>.
    /// </summary>
    [Serializable, PackerId("sample_life_steal")]
    public sealed class LifeStealModifier : SbModifier {
        [SerializeField] private float fraction = 0.3f;

        public override void Apply(ICanBeModified target) {
            if (TryGetBehaviour<LifeStealBehaviour>(target, out var lifeSteal))
                lifeSteal.Fraction = fraction;
        }

        public override void Remove(ICanBeModified target) {
            if (TryGetBehaviour<LifeStealBehaviour>(target, out var lifeSteal))
                lifeSteal.Fraction = 0f;
        }

        public override void Pack(ref Span<byte> buffer) => Packer.WriteFloat(ref buffer, fraction);
        public override void Unpack(ref ReadOnlySpan<byte> buffer) => fraction = Packer.ReadFloat(ref buffer);
        public override ISmartPacker CreateNewInstance() => new LifeStealModifier();
    }
}
