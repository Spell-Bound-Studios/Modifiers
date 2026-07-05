// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    [Serializable]
    public struct WeightedEntry<T> {
        public T candidate;
        public int weight;
    }
}