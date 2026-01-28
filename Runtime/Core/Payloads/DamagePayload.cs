using UnityEngine;

namespace Spellbound.Stats {
    /// <summary>
    /// Payload for damage events.
    /// </summary>
    public readonly struct DamagePayload {
        public readonly object Source;
        public readonly GameObject Target;
        public readonly float Amount;
        public readonly string DamageType;
        public readonly bool WasCrit;
        
        public DamagePayload(object source, GameObject target, float amount, string damageType, bool wasCrit = false) {
            Source = source;
            Target = target;
            Amount = amount;
            DamageType = damageType;
            WasCrit = wasCrit;
        }
    }
}