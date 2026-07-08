// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;

namespace Spellbound.Modifiers {
    [Serializable]
    public struct RolledModifier : IPacker {
        public uint modifierHash;
        public uint sourceId;
        public BakedRoll[] baked;

        public bool TryApplyTo(Modifiable target) {
            var definition = ModifierRegistry.GetDefinition(modifierHash);

            if (definition == null) {
                Log.Warn($"RolledModifier: no modifier registered for hash {modifierHash}; nothing applied.");

                return false;
            }

            ApplyTo(target, definition);

            return true;
        }

        public void ApplyTo(Modifiable target, ModifierDefinition definition) {
            var contributions = definition.Contributions;

            for (var i = 0; i < contributions.Count; i++) {
                var spec = contributions[i];

                if (spec.Stat != null && spec.Magnitude != null)
                    spec.Magnitude.ApplyTo(target.Stats, new StatId(spec.Stat.Hash), spec.Type, sourceId,
                            BakedFor(spec.Stat.Hash));

                if (spec.PairedStat != null && spec.PairedMagnitude != null)
                    spec.PairedMagnitude.ApplyTo(target.Stats, new StatId(spec.PairedStat.Hash), spec.Type, sourceId,
                            BakedFor(spec.PairedStat.Hash));
            }
        }

        private readonly float BakedFor(uint statHash) {
            if (baked != null) {
                for (var i = 0; i < baked.Length; i++) {
                    if (baked[i].statHash == statHash)
                        return baked[i].value;
                }
            }

            return 0f;
        }

        public int RemoveFrom(Modifiable target) => target.RemoveSource(sourceId);

        public int PackedSize =>
                2 * sizeof(uint) + sizeof(int) + (baked?.Length ?? 0) * (sizeof(uint) + sizeof(float));

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteUInt(ref buffer, modifierHash);
            Packer.WriteUInt(ref buffer, sourceId);
            Packer.WriteInt(ref buffer, baked?.Length ?? 0);

            if (baked == null)
                return;

            for (var i = 0; i < baked.Length; i++)
                baked[i].Pack(ref buffer);
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            modifierHash = Packer.ReadUInt(ref buffer);
            sourceId = Packer.ReadUInt(ref buffer);
            var count = Packer.ReadInt(ref buffer);
            baked = new BakedRoll[count];

            for (var i = 0; i < count; i++)
                baked[i].Unpack(ref buffer);
        }

        public override string ToString() =>
                ModifierRegistry.TryGetName(modifierHash, out var name) ? $"{name} ({sourceId})" : $"#{modifierHash}";
    }
}
