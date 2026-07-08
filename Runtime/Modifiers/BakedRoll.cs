// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Modifiers {
    [Serializable]
    public struct BakedRoll : IPacker {
        public uint statHash;
        public float value;

        public int PackedSize => sizeof(uint) + sizeof(float);

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteUInt(ref buffer, statHash);
            Packer.WriteFloat(ref buffer, value);
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            statHash = Packer.ReadUInt(ref buffer);
            value = Packer.ReadFloat(ref buffer);
        }
    }
}
