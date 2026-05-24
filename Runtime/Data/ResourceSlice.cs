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
    /// One entry inside a <see cref="ResourceSlice"/>: the resource's stat id paired with its current, min, and
    /// max at the time the slice was crafted. Self-contained per entry so the receiver doesn't need to compute
    /// max — it's right there.
    /// </summary>
    [Serializable]
    public struct ResourceSliceEntry {
        public int id;
        public float current;
        public float min;
        public float max;

        public ResourceSliceEntry(int id, float current, float min, float max) {
            this.id = id;
            this.current = current;
            this.min = min;
            this.max = max;
        }

        public float Ratio => max > 0f ? current / max : 0f;

        public override string ToString() {
            var name = StatRegistry.TryGetName(id, out var n) ? n : $"#{id}";

            return $"{name}: {current:F2} / {max:F2}";
        }
    }

    /// <summary>
    /// Lightweight value-type DTO carrying the runtime state of a subset of an entity's resources — current
    /// value plus bounds per entry. Crafted via <see cref="ResourceContainer.GetSlice(string[])"/>, sent over
    /// the wire, applied back via <see cref="ResourceContainer.ApplySlice"/>.
    /// </summary>
    /// <remarks>
    /// Focused on resources only. If you want stats, use <see cref="StatSlice"/>. The two are orthogonal DTOs;
    /// callers crafting one don't pay the cost of the other.
    /// </remarks>
    public struct ResourceSlice : IDecodableData {
        public static readonly string ID = "resource_slice";

        public static class Context {
            public const byte Silent = 0;
            public const byte Up = 1;
            public const byte Down = 2;
            public const byte Depleted = 3;
            public const byte Spawned = 4;
        }

        public List<ResourceSliceEntry> Entries;

        public ResourceSlice(int capacity) {
            Entries = new List<ResourceSliceEntry>(capacity);
        }

        public int Count => Entries?.Count ?? 0;

        public string PackerId => ID;

        public IDecodableData GetEmptyData() => new ResourceSlice(0);

        public IDecodableData InvokeApplyDelta(
            IDecodableData delta, ObjectPreset preset, int surfaceIndex, out byte context) =>
                this.ApplyDelta((ResourceSlice)delta, preset, surfaceIndex, out context);

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

            foreach (var e in Entries) {
                Packer.WriteInt(ref buffer, e.id);
                Packer.WriteFloat(ref buffer, e.current);
                Packer.WriteFloat(ref buffer, e.min);
                Packer.WriteFloat(ref buffer, e.max);
            }
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            var count = Packer.ReadInt(ref buffer);
            Entries = new List<ResourceSliceEntry>(count);

            for (var i = 0; i < count; i++) {
                var id = Packer.ReadInt(ref buffer);
                var current = Packer.ReadFloat(ref buffer);
                var min = Packer.ReadFloat(ref buffer);
                var max = Packer.ReadFloat(ref buffer);
                Entries.Add(new ResourceSliceEntry(id, current, min, max));
            }
        }

        #region Lookups

        public float GetCurrent(int id) {
            if (Entries == null)
                return 0f;

            for (var i = 0; i < Entries.Count; i++) {
                if (Entries[i].id == id)
                    return Entries[i].current;
            }

            return 0f;
        }

        public float GetMin(int id) {
            if (Entries == null)
                return 0f;

            for (var i = 0; i < Entries.Count; i++) {
                if (Entries[i].id == id)
                    return Entries[i].min;
            }

            return 0f;
        }

        public float GetMax(int id) {
            if (Entries == null)
                return 0f;

            for (var i = 0; i < Entries.Count; i++) {
                if (Entries[i].id == id)
                    return Entries[i].max;
            }

            return 0f;
        }

        public float GetRatio(int id) {
            var max = GetMax(id);

            return max > 0f ? GetCurrent(id) / max : 0f;
        }

        /// <summary>
        /// Returns a new slice with the entry for <paramref name="id"/>'s current replaced. No-op if the
        /// resource isn't present (use <see cref="ResourceContainer.GetSlice(string[])"/> to assemble a fresh
        /// slice when you need to add new resources).
        /// </summary>
        public ResourceSlice WithCurrent(int id, float newCurrent) {
            var copy = new ResourceSlice(Entries?.Count ?? 0);

            if (Entries == null)
                return copy;

            for (var i = 0; i < Entries.Count; i++) {
                var e = Entries[i];

                if (e.id == id)
                    e.current = newCurrent;

                copy.Entries.Add(e);
            }

            return copy;
        }

        #endregion

        public override string ToString() {
            if (Entries == null || Entries.Count == 0)
                return "ResourceSlice[empty]";

            var sb = new StringBuilder("ResourceSlice[");

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