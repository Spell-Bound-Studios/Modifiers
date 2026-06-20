// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// The current that flows through an enemy's damage circuit. Stages rewrite <see cref="Incoming"/> in place
    /// and read defensive stats by name via <see cref="GetValue"/>, which routes to whichever behaviour owns the
    /// stat — so no stage knows where <c>fire_resistance</c> or <c>armor</c> lives. Behaviour-level reach (the
    /// shield pool, the life pool) goes through <see cref="Defender"/>. A struct that stays on the stack; its
    /// fields are references, so stage edits to the list propagate.
    /// </summary>
    public struct DamageContext {
        public List<StatAndValue> Incoming;
        public BehaviourContainer Defender;

        /// <summary>
        /// Who dealt this hit — so a reflect stage can fire damage back at them. Null for self-inflicted damage
        /// (the ignite DoT) and for a reflected copy itself, which is the loop guard: no attacker, no bounce-back.
        /// </summary>
        public PlayerController Attacker;

        /// <summary>
        /// Read a defensive stat the circuit needs, routed to its owning behaviour. This is the seam a future
        /// "ignore fire resistance" rides on — an ignored stat returns 0 here and the stage needs no special case.
        /// </summary>
        public readonly float GetValue(uint statHash) => Defender.GetValue(statHash);
    }
}
