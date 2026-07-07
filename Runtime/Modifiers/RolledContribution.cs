// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Modifiers {
    [Serializable]
    [PackerId("rolled_contribution")]
    public struct RolledContribution : IRolledModifier {
        public uint statHash;
        public ContributionType type;
        public uint sourceStatHash;
        public float value;
        public uint sourceId;

        public uint SourceId => sourceId;

        public uint Hash => SmartPackerRegistry.GetHash(GetType());

        public ISmartPacker CreateNewInstance() => new RolledContribution();

        public bool TryApplyTo(Modifiable target) {
            var stat = new StatId(statHash);

            if (sourceStatHash != 0u)
                target.Stats.AddDerived(stat, type, new StatId(sourceStatHash), value, sourceId);
            else
                target.Stats.AddContribution(stat, type, value, sourceId);

            return true;
        }

        public int RemoveFrom(Modifiable target) => target.RemoveSource(sourceId);

        public int PackedSize => 3 * sizeof(uint) + sizeof(int) + sizeof(float);

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteUInt(ref buffer, statHash);
            Packer.WriteInt(ref buffer, (int)type);
            Packer.WriteUInt(ref buffer, sourceStatHash);
            Packer.WriteFloat(ref buffer, value);
            Packer.WriteUInt(ref buffer, sourceId);
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            statHash = Packer.ReadUInt(ref buffer);
            type = (ContributionType)Packer.ReadInt(ref buffer);
            sourceStatHash = Packer.ReadUInt(ref buffer);
            value = Packer.ReadFloat(ref buffer);
            sourceId = Packer.ReadUInt(ref buffer);
        }

        public override string ToString() {
            var name = StatRegistry.GetName(statHash);

            return string.IsNullOrEmpty(name) ? $"#{statHash} {value:0.###}" : $"{name} {value:0.###} ({sourceId})";
        }
    }
}
