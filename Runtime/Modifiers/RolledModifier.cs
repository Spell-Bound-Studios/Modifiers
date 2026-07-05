// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;

namespace Spellbound.Modifiers {
    [Serializable]
    public struct RolledModifier : IPacker {
        public uint modifierHash;
        public uint sourceId;
        public float[] values;

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
            var count = Math.Min(contributions.Count, values?.Length ?? 0);

            for (var i = 0; i < count; i++) {
                var range = contributions[i];
                target.Stats.AddContribution(new StatId(range.stat.Hash), range.type, values[i], sourceId);
            }
        }

        public int RemoveFrom(Modifiable target) => target.RemoveSource(sourceId);

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteUInt(ref buffer, modifierHash);
            Packer.WriteUInt(ref buffer, sourceId);
            Packer.WriteInt(ref buffer, values?.Length ?? 0);

            if (values == null)
                return;

            for (var i = 0; i < values.Length; i++)
                Packer.WriteFloat(ref buffer, values[i]);
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            modifierHash = Packer.ReadUInt(ref buffer);
            sourceId = Packer.ReadUInt(ref buffer);
            var count = Packer.ReadInt(ref buffer);
            values = new float[count];

            for (var i = 0; i < count; i++)
                values[i] = Packer.ReadFloat(ref buffer);
        }

        public override string ToString() =>
                ModifierRegistry.TryGetName(modifierHash, out var name) ? $"{name} ({sourceId})" : $"#{modifierHash}";
    }
}
