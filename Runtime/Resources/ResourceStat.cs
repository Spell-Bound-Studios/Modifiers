// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    public class ResourceStat {
        public float Min;
        public float Current;

        public ResourceStat(float current, float min = 0) {
            Current = current;
            Min = min;
        }
    }
}