// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour: banks killing blows, but only while <see cref="EmpowermentEnabled"/> — that flag is the
    /// opt-in the Empower-on-kill modifier flips. Owns <c>killing_blow</c> (seeds to 0). A target reports a
    /// killing blow in the consequence it returns; <see cref="Receive"/> banks it and the next cast spends one
    /// via <see cref="TrySpendEmpowerment"/>. A stat-gated reaction — no events, just a stat going up and down.
    /// </summary>
    [Serializable]
    public sealed class AwardBehaviour : SbBehaviour {
        private static uint? _killingBlowHash;
        private static uint KillingBlowHash => _killingBlowHash ??= StatRegistry.GetHash("killing_blow");

        private bool _empowermentEnabled;

        public override IReadOnlyList<StatAndValue> DeclareOwnedStats() => new[] { OwnedStat("killing_blow", 0f) };

        public float Banked => GetValue(KillingBlowHash);

        /// <summary>
        /// Opt-in switch (the Empower-on-kill modifier owns it). Only while enabled do kills bank and casts
        /// empower; disabling clears any banked blow so it can't carry across an unequip.
        /// </summary>
        public bool EmpowermentEnabled {
            get => _empowermentEnabled;
            set {
                _empowermentEnabled = value;

                if (!value)
                    SetBase("killing_blow", 0f);
            }
        }

        /// <summary>Bank any <c>killing_blow</c> carried in a consequence a target returned.</summary>
        public void Receive(List<StatAndValue> consequence) {
            if (!_empowermentEnabled)
                return;

            foreach (var entry in consequence) {
                if (entry.statHash == KillingBlowHash)
                    SetBase("killing_blow", Banked + entry.amount);
            }
        }

        /// <summary>If empowerment is enabled and a killing blow is banked, spend one and return true.</summary>
        public bool TrySpendEmpowerment() {
            if (!_empowermentEnabled || Banked < 1f)
                return false;

            SetBase("killing_blow", Banked - 1f);

            return true;
        }
    }
}
