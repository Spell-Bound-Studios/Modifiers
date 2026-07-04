// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Modifiers {
    [Serializable]
    public struct StatAndValue : IPacker {
        public uint statHash;
        public float amount;

        public StatAndValue(uint statHash, float amount) {
            this.statHash = statHash;
            this.amount = amount;
        }

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteUInt(ref buffer, statHash);
            Packer.WriteFloat(ref buffer, amount);
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            statHash = Packer.ReadUInt(ref buffer);
            amount = Packer.ReadFloat(ref buffer);
        }

        public override string ToString() =>
                StatRegistry.TryGetName(statHash, out var n) ? $"{n}={amount}" : $"#{statHash}={amount}";
    }
}
