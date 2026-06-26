// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample modifier: adds a flat amount to <c>projectile_count</c> on whatever owns a
    /// <see cref="ProjectileBehaviour"/>. A pure stat change — the next cast simply fires more projectiles.
    /// </summary>
    [Serializable, PackerId("sample_added_projectile_count")]
    public sealed class AddedProjectileCountModifier : SbModifier {
        [SerializeField] private int additional = 2;

        public override void Apply(ICanBeModified target) {
            if (TryGetBehaviour<ProjectileBehaviour>(target, out var projectile))
                projectile.AddFlat("sample_projectile_count", additional, UniqueId);
        }

        public override void Remove(ICanBeModified target) {
            if (TryGetBehaviour<ProjectileBehaviour>(target, out var projectile))
                projectile.RemoveModifierByUniqueId(UniqueId);
        }

        public override void Pack(ref Span<byte> buffer) => Packer.WriteInt(ref buffer, additional);
        public override void Unpack(ref ReadOnlySpan<byte> buffer) => additional = Packer.ReadInt(ref buffer);
        public override ISmartPacker CreateNewInstance() => new AddedProjectileCountModifier();
    }
}
