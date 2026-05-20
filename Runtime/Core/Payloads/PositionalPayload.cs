// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Immutable payload for positional events — "something happened at a place, facing a direction." Used
    /// by skill activations, beam fires, projectile spawns, and any other event whose semantics are a point
    /// plus a forward vector. <see cref="Source"/> carries the originator (often a behaviour or skill) for
    /// modifiers that filter by who's firing.
    /// </summary>
    public readonly struct PositionalPayload {
        public readonly object Source;
        public readonly Vector3 Position;
        public readonly Vector3 Direction;

        public PositionalPayload(object source, Vector3 position, Vector3 direction) {
            Source = source;
            Position = position;
            Direction = direction;
        }
    }
}