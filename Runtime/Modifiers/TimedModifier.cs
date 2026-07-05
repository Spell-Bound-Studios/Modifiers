// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Modifiers {
    [Serializable]
    public struct TimedModifier : IPacker {
        public RolledModifier modifier;
        public float duration;
        public float remaining;

        public int PackedSize => modifier.PackedSize + 2 * sizeof(float);

        public void Pack(ref Span<byte> buffer) {
            modifier.Pack(ref buffer);
            Packer.WriteFloat(ref buffer, duration);
            Packer.WriteFloat(ref buffer, remaining);
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            modifier.Unpack(ref buffer);
            duration = Packer.ReadFloat(ref buffer);
            remaining = Packer.ReadFloat(ref buffer);
        }

        public override string ToString() => $"{modifier} {remaining:0.#}/{duration:0.#}s";
    }
}
