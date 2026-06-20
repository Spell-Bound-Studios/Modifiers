// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample modifier: flips the <c>IgniteEnabled</c> capability on a <see cref="FireBehaviour"/> so its hits
    /// also light the target on fire. A pure capability toggle — no stat, no number, the behaviour just gains
    /// a new thing it does.
    /// </summary>
    [Serializable, PackerId("sample_ignite")]
    public sealed class IgniteModifier : SbModifier {
        public override void Apply(ICanBeModified target) {
            if (TryGetBehaviour<FireBehaviour>(target, out var fire))
                fire.IgniteEnabled = true;
        }

        public override void Remove(ICanBeModified target) {
            if (TryGetBehaviour<FireBehaviour>(target, out var fire))
                fire.IgniteEnabled = false;
        }

        public override void Pack(ref Span<byte> buffer) { }
        public override void Unpack(ref ReadOnlySpan<byte> buffer) { }
        public override ISmartPacker CreateNewInstance() => new IgniteModifier();
    }
}
