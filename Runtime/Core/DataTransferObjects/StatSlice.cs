// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Text;
using Spellbound.Core;
using Spellbound.Core.ObjectData;
using Spellbound.Core.ObjectHandling;
using Spellbound.Core.Objects;
using Spellbound.Core.Packing;

namespace Spellbound.Modifiers {
    /// <summary>
    /// One entry inside a <see cref="StatSlice"/>: a stat's runtime id paired with its computed value at the
    /// time the slice was crafted. Ids (not names) ride the wire because slices are ephemeral tick DTOs and
    /// the registry is deterministic when both ends share the same loaded <see cref="StatDatabase"/>.
    /// </summary>
    [Serializable]
    public struct StatSliceEntry {
        public int id;
        public float value;

        public StatSliceEntry(int id, float value) {
            this.id = id;
            this.value = value;
        }

        public override string ToString() => $"#{id}: {value:F2}";
    }

    /// <summary>
    /// Lightweight value-type DTO carrying a subset of a <see cref="StatContainer"/>'s computed values, packable
    /// for network transmission and chunk-data persistence. Use <see cref="StatContainer.GetSlice(string[])"/>
    /// or <see cref="StatContainer.GetSlice(int[])"/> to craft one.
    /// </summary>
    public struct StatSlice : IDecodableData {
        public static readonly string ID = "stat_slice";

        /// <summary>
        /// Look up a stat's value by registry id. Hot-path safe — no string work. Returns 0 if the slice
        /// has no entry for the id (or is uninitialized).
        /// </summary>
        public float GetStatValue(int id) {
            if (Entries == null)
                return 0f;

            for (var i = 0; i < Entries.Count; i++) {
                if (Entries[i].id == id)
                    return Entries[i].value;
            }

            return 0f;
        }

        /// <summary>
        /// Look up a stat's value by name. Convenience wrapper that resolves the name to an id once;
        /// prefer the int overload in hot paths where the id is already in hand.
        /// </summary>
        public float GetStatValue(string statName) => GetStatValue(StatRegistry.GetId(statName));

        /// <summary>
        /// Return a new slice equal to this one but with the entry for <paramref name="id"/> replaced
        /// (or appended if absent). The original slice is not mutated.
        /// </summary>
        public StatSlice WithStatValue(int id, float value) {
            var copy = new StatSlice((Entries?.Count ?? 0) + 1);
            var written = false;

            if (Entries != null) {
                for (var i = 0; i < Entries.Count; i++) {
                    var e = Entries[i];

                    if (e.id == id) {
                        copy.Entries.Add(new StatSliceEntry(id, value));
                        written = true;
                    }
                    else
                        copy.Entries.Add(e);
                }
            }

            if (!written)
                copy.Entries.Add(new StatSliceEntry(id, value));

            return copy;
        }

        /// <summary>
        /// Name-based convenience wrapper around <see cref="WithStatValue(int, float)"/>.
        /// </summary>
        public StatSlice WithStatValue(string statName, float value) =>
                WithStatValue(StatRegistry.GetId(statName), value);

        public List<StatSliceEntry> Entries;

        public StatSlice(int capacity) {
            Entries = new List<StatSliceEntry>(capacity);
        }

        public int Count => Entries?.Count ?? 0;

        public string PackerId => ID;

        public IDecodableData GetEmptyData() => new StatSlice(0);

        public IDecodableData InvokeApplyDelta(
            IDecodableData delta, ObjectPreset preset, int surfaceIndex, out byte context) =>
                this.ApplyDelta((StatSlice)delta, preset, surfaceIndex, out context);

        public IDecodableData InvokeGetDefaultData(ObjectPreset preset, int surfaceIndex, byte level = 1) =>
                this.GetDefaultData(preset, surfaceIndex, level);

        public void InvokeChangeCallback(
            byte context, ObjectParent parent, int instanceIndex,
            ObjectPreset preset, int surfaceIndex, TransformData transformData) =>
                this.ChangeCallback(context, parent, instanceIndex, preset, surfaceIndex, transformData);

        public void InvokeResolveCallback(
            byte context, ObjectParent parent, int instanceIndex,
            ObjectPreset preset, int surfaceIndex, TransformData transformData) =>
                this.ResolveCallback(context, parent, instanceIndex, preset, surfaceIndex, transformData);

        public void Pack(ref Span<byte> buffer) {
            var count = Entries?.Count ?? 0;
            Packer.WriteInt(ref buffer, count);

            if (Entries == null)
                return;

            for (var i = 0; i < Entries.Count; i++) {
                Packer.WriteInt(ref buffer, Entries[i].id);
                Packer.WriteFloat(ref buffer, Entries[i].value);
            }
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            var count = Packer.ReadInt(ref buffer);
            Entries = new List<StatSliceEntry>(count);

            for (var i = 0; i < count; i++) {
                var id = Packer.ReadInt(ref buffer);
                var value = Packer.ReadFloat(ref buffer);
                Entries.Add(new StatSliceEntry(id, value));
            }
        }

        public override string ToString() {
            if (Entries == null || Entries.Count == 0)
                return "StatSlice[empty]";

            var sb = new StringBuilder("StatSlice[");

            for (var i = 0; i < Entries.Count; i++) {
                if (i > 0)
                    sb.Append(", ");

                sb.Append(Entries[i]);
            }

            sb.Append(']');

            return sb.ToString();
        }
    }
}