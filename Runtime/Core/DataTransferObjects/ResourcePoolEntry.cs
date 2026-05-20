// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    public struct ResourcePoolEntry {
        public readonly float Value;
        public readonly float Max;

        public ResourcePoolEntry(float maxValue) {
            Value = maxValue;
            Max = maxValue;
        }
        
        public ResourcePoolEntry(float currentValue, float maxValue) {
            Value = currentValue;
            Max = maxValue;
        }

        public override string ToString() => $"{Value:F2}";
    }
}
