// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample modifier: flips the <c>SplitOnHit</c> capability on a <see cref="ProjectileBehaviour"/> so each
    /// projectile fans into more on impact. Like <see cref="IgniteModifier"/>, a capability toggle — it changes
    /// what the behaviour does, not a stat number.
    /// </summary>
    [Serializable, PackerId("sample_split_on_hit")]
    public sealed class SplitOnHitModifier : SbModifier {
        public override void Apply(ICanBeModified target) {
            if (TryGetBehaviour<ProjectileBehaviour>(target, out var projectile))
                projectile.SplitOnHit = true;
        }

        public override void Remove(ICanBeModified target) {
            if (TryGetBehaviour<ProjectileBehaviour>(target, out var projectile))
                projectile.SplitOnHit = false;
        }

        public override void Pack(ref Span<byte> buffer) { }
        public override void Unpack(ref ReadOnlySpan<byte> buffer) { }
        public override ISmartPacker CreateNewInstance() => new SplitOnHitModifier();
    }
}
