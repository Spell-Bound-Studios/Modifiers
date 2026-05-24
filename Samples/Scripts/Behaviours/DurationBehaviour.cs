// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour: holds the three duration stats a sample skill might care about — ignite duration,
    /// chill duration, and the umbrella skill duration. Modifiers like
    /// <see cref="IncreasedDurationModifier"/> target these stats; the skill / DoT system reads them when it
    /// needs to know how long an effect lasts.
    /// </summary>
    [Serializable]
    public sealed class DurationBehaviour : SbBehaviour {
        public float GetIgniteDuration() => GetValue("ignite_duration");
        public float GetChillDuration() => GetValue("chill_duration");
        public float GetSkillDuration() => GetValue("skill_duration");
    }
}