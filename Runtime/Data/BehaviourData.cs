// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core;
using Spellbound.Core.ObjectData;
using Spellbound.Core.ObjectHandling;
using Spellbound.Core.Objects;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// The data that belongs to a behaviour container. It is intended to represent all of the networkable and savable
    /// data per container instance.
    /// </summary>
    public struct BehaviourData : IPackerObjectData {
        public List<ResourceData> resourceData; // Stat max, min, current
        public List<StatAndValue> statModifiers; // Affix and Traits
        // Buffs
        // Debuffs
        
        
        
        
        
        public void Pack(ref Span<byte> buffer) => throw new NotImplementedException();

        public void Unpack(ref ReadOnlySpan<byte> buffer) => throw new NotImplementedException();
        public uint Hash { get; }
        public ISmartPacker CreateNewInstance() => throw new NotImplementedException();

        public IPackerObjectData GetEmptyData() => throw new NotImplementedException();

        public void InvokeChangeCallback(
            byte context, ObjectParent parent, int instanceIndex, ObjectPreset preset, int surfaceIndex,
            TransformData transformData) =>
                throw new NotImplementedException();

        public void InvokeResolveCallback(
            byte context, ObjectParent parent, int instanceIndex, ObjectPreset preset, int surfaceIndex,
            TransformData transformData) =>
                throw new NotImplementedException();
    }
}