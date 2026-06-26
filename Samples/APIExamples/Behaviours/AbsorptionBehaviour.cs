// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour: a shield pool that eats damage before mitigation. Max is the live <c>shield</c> stat
    /// (so a +shield buff just works); current is tracked here and drained by <see cref="Absorb"/>. Fills to
    /// max on first read.
    /// </summary>
    [Serializable]
    public sealed class AbsorptionBehaviour : SbBehaviour {
        private static uint? _shieldHash;
        private static uint ShieldHash => _shieldHash ??= StatRegistry.GetHash("shield");

        private float _current;
        private bool _initialized;

        public override IReadOnlyList<StatAndValue> DeclareOwnedStats() => new[] { OwnedStat("shield", 50f) };

        public float Max => GetValue(ShieldHash);

        public float Current {
            get {
                EnsureInitialized();

                return _current;
            }
        }

        /// <summary>Absorb up to <paramref name="amount"/>, drain the pool by what it took, and return that.</summary>
        public float Absorb(float amount) {
            EnsureInitialized();

            var taken = Math.Min(_current, amount);
            _current -= taken;

            return taken;
        }

        /// <summary>Refill the shield to full — used on respawn.</summary>
        public void Reset() {
            _initialized = true;
            _current = Max;
        }

        private void EnsureInitialized() {
            if (_initialized)
                return;

            _initialized = true;
            _current = Max;
        }
    }
}
