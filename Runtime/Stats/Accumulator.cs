// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    public struct Accumulator {
        private int _flat;          // Σ Flat, internal units
        private int _increased;     // Σ Increased, internal units (Precision == 100%)
        private long _moreProduct;  // ∏ (1 + More), scaled by Precision
        private bool _hasMore;
        private bool _hasOverride;
        private int _override;      // internal units

        public void AddFlat(int internalValue) => _flat += internalValue;

        public void AddIncreased(int internalValue) => _increased += internalValue;

        public void MultiplyMore(int internalValue) {
            var precision = StatSettings.Precision;

            if (!_hasMore) {
                _hasMore = true;
                _moreProduct = precision;
            }

            _moreProduct = _moreProduct * Math.Max(0, precision + internalValue) / precision;
        }

        public void SetOverride(int internalValue) {
            if (_hasOverride && _override <= internalValue)
                return;

            _hasOverride = true;
            _override = internalValue;
        }

        public void Merge(in Accumulator other) {
            _flat += other._flat;
            _increased += other._increased;

            if (other._hasMore) {
                if (!_hasMore) {
                    _hasMore = true;
                    _moreProduct = StatSettings.Precision;
                }

                _moreProduct = _moreProduct * other._moreProduct / StatSettings.Precision;
            }

            if (other._hasOverride)
                SetOverride(other._override);
        }

        public void Apply(ModifierType type, int internalValue) {
            switch (type) {
                case ModifierType.Flat:
                    AddFlat(internalValue);
                    break;
                case ModifierType.Increased:
                    AddIncreased(internalValue);
                    break;
                case ModifierType.More:
                    MultiplyMore(internalValue);
                    break;
                case ModifierType.Override:
                    SetOverride(internalValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public int Resolve(int baseInternal) {
            if (_hasOverride)
                return _override;

            var precision = StatSettings.Precision;

            long value = baseInternal + _flat;
            value = value * Math.Max(0, precision + _increased) / precision;

            var more = _hasMore ? _moreProduct : precision;
            value = value * more / precision;

            return (int)value;
        }
    }
}
