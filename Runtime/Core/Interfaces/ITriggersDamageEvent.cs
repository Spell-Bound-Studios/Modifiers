using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Implement this on anything that deals damage.
    /// The implementer is responsible for invoking OnDamageDealt when damage is dealt.
    /// </summary>
    public interface ITriggersDamageEvent {
        Action<DamagePayload> OnDamageDealt { get; set; }
    }
}