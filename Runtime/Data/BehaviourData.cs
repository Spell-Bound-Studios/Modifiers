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
    /// The data that belongs to a behaviour container. It is intended to represent all of the networkable and savable
    /// data per container instance.
    /// </summary>
    [PackerId("behaviour_data")]
    public struct BehaviourData : IPackerObjectData {
        public List<ResourceData> resourceData; // Stat max, min, current
        public List<StatAndValue> statModifiers; // Affix and Traits
        // Buffs
        // Debuffs

        #region IPackerObjectData

        public IPackerObjectData GetEmptyData() => new BehaviourData();

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
            Packer.PackList(ref buffer, statModifiers);
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            resourceData = Packer.UnpackList<ResourceData>(ref buffer);
            statModifiers = Packer.UnpackList<StatAndValue>(ref buffer);
        }

        #endregion IPacker
        
        #region IRegistryEntry
        
        public uint Hash => SmartPackerRegistry.GetHash(GetType());
        
        #endregion IRegistryEntry
        
        #region ISmartPacker
        
        public ISmartPacker CreateNewInstance() => new BehaviourData();

        #endregion ISmartPacker

        #region ToString
        
        public override string ToString() {
            var sb = new StringBuilder(64);
            sb.Append("BehaviourData [resources: ").Append(ResourceCount)
              .Append(", modifiers: ").Append(ModifierCount)
              .Append(", packed: ").Append(PackedSize).Append(" B]");

            if (resourceData != null) {
                for (var i = 0; i < resourceData.Count; i++)
                    sb.Append("\n    res[").Append(i).Append("] ").Append(resourceData[i]);
            }

            if (statModifiers == null) return sb.ToString();

            {
                for (var i = 0; i < statModifiers.Count; i++)
                    sb.Append("\n    mod[").Append(i).Append("] ").Append(statModifiers[i]);
            }

            return sb.ToString();
        }

        #endregion ToString
        
        #region Queries
        
        private const int CountPrefixBytes = sizeof(int);                   // length prefix per list
        private const int ResourceBytes = sizeof(uint) + 3 * sizeof(float); // statHash + max + min + current
        private const int ModifierBytes = sizeof(uint) + sizeof(float);     // statHash + amount

        /// <summary>
        /// Number of resource entries.
        /// </summary>
        public int ResourceCount => resourceData?.Count ?? 0;

        /// <summary>
        /// Number of stat modifier entries.
        /// </summary>
        public int ModifierCount => statModifiers?.Count ?? 0;

        /// <summary>
        /// Total number of packed entries across both lists.
        /// </summary>
        public int Count => ResourceCount + ModifierCount;

        /// <summary>
        /// True when there is no data to pack on either list.
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        /// Exact size in bytes this struct will occupy once packed, computed without allocating. Mirrors the
        /// layout written by <see cref="Pack"/>; keep the two in sync if either list's element shape changes.
        /// </summary>
        public int PackedSize =>
                CountPrefixBytes + ResourceCount * ResourceBytes +
                CountPrefixBytes + ModifierCount * ModifierBytes;

        #endregion Queries
    }
}