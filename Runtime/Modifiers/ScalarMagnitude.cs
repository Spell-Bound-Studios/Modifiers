// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    [Serializable]
    public abstract class ScalarMagnitude : Magnitude {
        public abstract float Value(float baked);

        public override void ApplyTo(StatBlock stats, StatId stat, ContributionType type, uint sourceId, float baked) =>
                stats.AddContribution(stat, type, Value(baked), sourceId);
    }
}
