using UnityEngine;

namespace Spellbound.Modifiers {
    public readonly struct TargetedPayload {
        public readonly object Source;
        public readonly GameObject Target;
        public readonly Vector3 Position;
        public readonly object Cause;
        
        public TargetedPayload(object source, GameObject target, Vector3 position, object cause = null) {
            Source = source;
            Target = target;
            Position = position;
            Cause = cause;
        }
    }
}