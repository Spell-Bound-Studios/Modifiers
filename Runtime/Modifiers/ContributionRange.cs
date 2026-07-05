// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    [Serializable]
    public struct ContributionRange {
        public StatDefinition stat;
        public ContributionType type;
        public float min;
        public float max;
        public float step; // roll granularity: 1 rolls integers, 0.5 rolls halves, 0 is continuous
    }
}