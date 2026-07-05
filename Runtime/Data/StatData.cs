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
    /// The data that belongs to a stat container. It is intended to represent all of the networkable and savable
    /// data per container instance.
    /// </summary>
    [PackerId("stat_data")]
    public struct StatData : IPackerObjectData {
        public List<ResourceData> resourceData; // Stat max, min, current
        public List<RolledModifier> modifiers;
        public List<TimedModifier> buffs;
        public List<TimedModifier> debuffs;

        public static class Context {
            public const byte Silent = 0;
            public const byte ResourcesGained = 1;
            public const byte ResourcesLost = 2;
            public const byte ResourcesUnchanged = 3;
            public const byte Died = 4;
        }

        #region Hydration

        public readonly void ApplyTo(Modifiable target, TimedModifierSet buffSet = null, TimedModifierSet debuffSet = null) {
            if (modifiers != null) {
                for (var i = 0; i < modifiers.Count; i++)
                    modifiers[i].TryApplyTo(target);
            }

            if (buffSet != null && buffs != null) {
                for (var i = 0; i < buffs.Count; i++)
                    buffSet.Restore(buffs[i]);
            }

            if (debuffSet != null && debuffs != null) {
                for (var i = 0; i < debuffs.Count; i++)
                    debuffSet.Restore(debuffs[i]);
            }
        }

        public static StatData Capture(
                List<ResourceData> resources, List<RolledModifier> applied,
                TimedModifierSet buffSet = null, TimedModifierSet debuffSet = null) =>
                new() {
                    resourceData = resources,
                    modifiers = applied != null ? new List<RolledModifier>(applied) : null,
                    buffs = Snapshot(buffSet),
                    debuffs = Snapshot(debuffSet)
                };

        private static List<TimedModifier> Snapshot(TimedModifierSet set) {
            if (set == null || set.Active.Count == 0)
                return null;

            return new List<TimedModifier>(set.Active);
        }

        #endregion Hydration

        #region IPackerObjectData

        public IPackerObjectData GetEmptyData() => new StatData();

        public void InvokeChangeCallback(
            byte context, ObjectParent parent, int instanceIndex, ObjectPreset preset, byte surfaceIndex,
            TransformData transformData) =>
                this.ChangeCallback(context, parent, instanceIndex, preset, surfaceIndex, transformData);

        public void InvokeResolveCallback(
            byte context, ObjectParent parent, int instanceIndex, ObjectPreset preset, byte surfaceIndex,
            TransformData transformData) =>
                this.ResolveCallback(context, parent, instanceIndex, preset, surfaceIndex, transformData);

        #endregion IPackerObjectData

        #region IPacker

        public void Pack(ref Span<byte> buffer) {
            Packer.PackList(ref buffer, resourceData);
            Packer.PackList(ref buffer, modifiers);
            Packer.PackList(ref buffer, buffs);
            Packer.PackList(ref buffer, debuffs);
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            resourceData = Packer.UnpackList<ResourceData>(ref buffer);
            modifiers = Packer.UnpackList<RolledModifier>(ref buffer);
            buffs = Packer.UnpackList<TimedModifier>(ref buffer);
            debuffs = Packer.UnpackList<TimedModifier>(ref buffer);
        }

        #endregion IPacker

        #region IRegistryEntry

        public uint Hash => SmartPackerRegistry.GetHash(GetType());

        #endregion IRegistryEntry

        #region ISmartPacker

        public ISmartPacker CreateNewInstance() => new StatData();

        #endregion ISmartPacker

        #region ToString

        public override string ToString() {
            var sb = new StringBuilder(64);
            sb.Append("StatData [resources: ").Append(ResourceCount)
              .Append(", modifiers: ").Append(ModifierCount)
              .Append(", buffs: ").Append(BuffCount)
              .Append(", debuffs: ").Append(DebuffCount)
              .Append(", packed: ").Append(PackedSize).Append(" B]");

            for (var i = 0; i < ResourceCount; i++)
                sb.Append("\n    res[").Append(i).Append("] ").Append(resourceData[i]);

            for (var i = 0; i < ModifierCount; i++)
                sb.Append("\n    mod[").Append(i).Append("] ").Append(modifiers[i]);

            for (var i = 0; i < BuffCount; i++)
                sb.Append("\n    buff[").Append(i).Append("] ").Append(buffs[i]);

            for (var i = 0; i < DebuffCount; i++)
                sb.Append("\n    debuff[").Append(i).Append("] ").Append(debuffs[i]);

            return sb.ToString();
        }

        #endregion ToString

        #region Queries

        private const int CountPrefixBytes = sizeof(int);                   // length prefix per list
        private const int ResourceBytes = sizeof(uint) + 3 * sizeof(float); // statHash + max + min + current

        /// <summary>
        /// Number of resource entries.
        /// </summary>
        public int ResourceCount => resourceData?.Count ?? 0;

        /// <summary>
        /// Number of rolled modifier entries.
        /// </summary>
        public int ModifierCount => modifiers?.Count ?? 0;

        public int BuffCount => buffs?.Count ?? 0;

        public int DebuffCount => debuffs?.Count ?? 0;

        /// <summary>
        /// Total number of packed entries across all lists.
        /// </summary>
        public int Count => ResourceCount + ModifierCount + BuffCount + DebuffCount;

        /// <summary>
        /// True when there is no data to pack on any list.
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        /// Exact size in bytes this struct will occupy once packed, computed without allocating. Mirrors the
        /// layout written by <see cref="Pack"/>; keep the two in sync if either list's element shape changes.
        /// </summary>
        public int PackedSize {
            get {
                var size = 4 * CountPrefixBytes + ResourceCount * ResourceBytes;

                for (var i = 0; i < ModifierCount; i++)
                    size += modifiers[i].PackedSize;

                for (var i = 0; i < BuffCount; i++)
                    size += buffs[i].PackedSize;

                for (var i = 0; i < DebuffCount; i++)
                    size += debuffs[i].PackedSize;

                return size;
            }
        }

        #endregion Queries
    }
}
