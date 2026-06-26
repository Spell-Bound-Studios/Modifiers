// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour: a single health pool. Max is the live <c>health</c> stat (so a +max-health buff just
    /// works); current is tracked here, drained by <see cref="Apply"/> / <see cref="Damage"/> and refilled by
    /// <see cref="Heal"/> (life steal). Fills to max on first read.
    /// </summary>
    [Serializable]
    public sealed class ResourceBehaviour : SbBehaviour {
        private static uint? _healthHash;
        private static uint HealthHash => _healthHash ??= StatRegistry.GetHash("sample_health");

        private float _current;
        private bool _initialized;

        public override IReadOnlyList<StatAndValue> DeclareOwnedStats() => new[] { OwnedStat("sample_health", 100f) };

        public float Max => GetValue(HealthHash);

        public float Current {
            get {
                EnsureInitialized();

                return _current;
            }
        }

        public bool IsDead => Current <= 0f;

        /// <summary>Drain the pool by the summed amount of an already-mitigated damage list.</summary>
        public void Apply(List<StatAndValue> damage) {
            var total = 0f;

            foreach (var entry in damage)
                total += entry.amount;

            Damage(total);
        }

        /// <summary>Drain the pool by a flat amount, clamped at zero.</summary>
        public void Damage(float amount) {
            EnsureInitialized();
            _current = Math.Max(0f, _current - amount);
        }

        /// <summary>Refill the pool by a flat amount, clamped at max — what life steal restores.</summary>
        public void Heal(float amount) {
            EnsureInitialized();
            _current = Math.Min(Max, _current + amount);
        }

        /// <summary>Refill the pool to full — used on respawn.</summary>
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
