// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    [Serializable]
    public abstract class Magnitude {
        public virtual bool Rolls => false;

        public virtual bool IsValid => true;

        public virtual float Bake(System.Random rng) => 0f;

        public abstract void ApplyTo(StatBlock stats, StatId stat, ContributionType type, uint sourceId, float baked);
    }
}
