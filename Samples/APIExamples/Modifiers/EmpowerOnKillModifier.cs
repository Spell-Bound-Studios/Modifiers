// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample modifier: opts a skill into kill-empowerment by enabling its <see cref="AwardBehaviour"/>. With it
    /// equipped, a killing blow banks and empowers the next cast (which flies green); without it, kills do
    /// nothing. A capability toggle — the empowerment is never implicit, it's a modifier you choose.
    /// </summary>
    [Serializable, PackerId("sample_empower_on_kill")]
    public sealed class EmpowerOnKillModifier : SbModifier {
        public override void Apply(ICanBeModified target) {
            if (TryGetBehaviour<AwardBehaviour>(target, out var award))
                award.EmpowermentEnabled = true;
        }

        public override void Remove(ICanBeModified target) {
            if (TryGetBehaviour<AwardBehaviour>(target, out var award))
                award.EmpowermentEnabled = false;
        }

        public override void Pack(ref Span<byte> buffer) { }
        public override void Unpack(ref ReadOnlySpan<byte> buffer) { }
        public override ISmartPacker CreateNewInstance() => new EmpowerOnKillModifier();
    }
}
