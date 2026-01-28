using UnityEngine;

namespace Spellbound.Stats {
    public readonly struct TargetedPayload {
        public readonly object Source;
        public readonly GameObject Target;
        public readonly Vector3 Position;
        
        public TargetedPayload(object source, GameObject target, Vector3 position) {
            Source = source;
            Target = target;
            Position = position;
        }
    }
}