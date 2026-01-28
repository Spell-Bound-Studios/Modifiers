using UnityEngine;

namespace Spellbound.Stats {
    /// <summary>
    /// Payload for positional events.
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