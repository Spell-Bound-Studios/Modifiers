// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample modifier: adds N to <c>projectile_count</c> on whichever target owns a
    /// <see cref="ProjectileBehaviour"/>. The simplest possible <see cref="SbModifier"/> shape — find the
    /// behaviour, push a flat stat modifier, remove by <see cref="SbModifier.UniqueId"/> on detach. Use this
    /// pattern for every numeric "+N to X" affix.
    /// </summary>
    [Serializable]
    public sealed class AddedProjectileCountModifier : SbModifier {
        [SerializeField] private int additionalProjectiles = 6;

        public override void Apply(ICanBeModified target) {
            // I can use the built-in TryGetBehaviour helper to access the specific Behaviour I want out.
            if (!TryGetBehaviour<ProjectileBehaviour>(target, out var projectile))
                return;

            // Now I can actually add this modification to the targeted behaviour.
            // In this example modifier I simply want to increase a stat value on the behaviour in an additive way.
            projectile.AddFlat("projectile_count", additionalProjectiles, UniqueId);
        }

        public override void Remove(ICanBeModified target) {
            // I can leverage the same try get behaviour as I did in Apply().
            if (!TryGetBehaviour<ProjectileBehaviour>(target, out var projectile))
                return;

            // Then I can remove this modifier by its unique ID that we have access to via the SbModifier base class.
            projectile.RemoveModifierByUniqueId(UniqueId);
        }

        public override void Pack(ref Span<byte> buffer) =>
                Packer.WriteInt(ref buffer, additionalProjectiles);

        public override void Unpack(ref ReadOnlySpan<byte> buffer) =>
                additionalProjectiles = Packer.ReadInt(ref buffer);
    }
}
