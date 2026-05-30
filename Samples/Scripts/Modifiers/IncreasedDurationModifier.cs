// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample modifier: pushes an <see cref="ModifierType.Increased"/> entry onto <c>ignite_duration</c> on
    /// whichever target owns a <see cref="DurationBehaviour"/>. Demonstrates the additive % pool — multiple
    /// stacks of this modifier add their percents together (PoE "increased" math), not multiply.
    /// </summary>
    [Serializable]
    public sealed class IncreasedDurationModifier : SbModifier {
        [SerializeField] private float increasedDurationPercent = .5f;

        public override void Apply(ICanBeModified target) {
            if (!TryGetBehaviour<DurationBehaviour>(target, out var duration))
                return;

            duration.AddIncreased("ignite_duration", increasedDurationPercent, UniqueId);
        }

        public override void Remove(ICanBeModified target) {
            if (!TryGetBehaviour<DurationBehaviour>(target, out var duration))
                return;

            duration.RemoveModifierByUniqueId(UniqueId);
        }

        public override void Pack(ref Span<byte> buffer) => Packer.WriteFloat(ref buffer, increasedDurationPercent);

        public override void Unpack(ref ReadOnlySpan<byte> buffer) =>
                increasedDurationPercent = Packer.ReadFloat(ref buffer);
    }
}