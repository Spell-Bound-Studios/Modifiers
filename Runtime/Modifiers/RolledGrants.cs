// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Modifiers {
    /// <summary>
    /// The instance record of a rolled <see cref="ModifierGrantSet"/>: inline lines persist as stat-keyed
    /// baked rolls whose route back is the owning instance; named lines persist as
    /// <see cref="RolledModifier"/>s whose route back is the modifier hash. Save or send this next to the
    /// owner's other instance data and hydrate through <see cref="ModifierGrantSet.Apply"/>.
    /// </summary>
    [Serializable]
    public struct RolledGrants : IPacker {
        public BakedRoll[] baked;
        public RolledModifier[] modifiers;

        public bool IsEmpty =>
                (baked == null || baked.Length == 0) && (modifiers == null || modifiers.Length == 0);

        public int PackedSize {
            get {
                var size = 2 * sizeof(int) + (baked?.Length ?? 0) * (sizeof(uint) + sizeof(float));

                if (modifiers != null) {
                    for (var i = 0; i < modifiers.Length; i++)
                        size += modifiers[i].PackedSize;
                }

                return size;
            }
        }

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteInt(ref buffer, baked?.Length ?? 0);

            if (baked != null) {
                for (var i = 0; i < baked.Length; i++)
                    baked[i].Pack(ref buffer);
            }

            Packer.WriteInt(ref buffer, modifiers?.Length ?? 0);

            if (modifiers != null) {
                for (var i = 0; i < modifiers.Length; i++)
                    modifiers[i].Pack(ref buffer);
            }
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            var bakedCount = Packer.ReadInt(ref buffer);
            baked = new BakedRoll[bakedCount];

            for (var i = 0; i < bakedCount; i++)
                baked[i].Unpack(ref buffer);

            var modifierCount = Packer.ReadInt(ref buffer);
            modifiers = new RolledModifier[modifierCount];

            for (var i = 0; i < modifierCount; i++)
                modifiers[i].Unpack(ref buffer);
        }
    }
}
