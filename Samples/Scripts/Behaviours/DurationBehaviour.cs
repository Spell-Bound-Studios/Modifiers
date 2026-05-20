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
        [SerializeField] private float igniteDuration = 4f;
        [SerializeField] private float chillDuration = 4f;
        [SerializeField] private float skillDuration = 5f;

        public float GetIgniteDuration() => Stats.GetValue("ignite_duration");
        public float GetChillDuration() => Stats.GetValue("chill_duration");
        public float GetSkillDuration() => Stats.GetValue("skill_duration");

        protected override StatContainer InitializeStats() {
            var stats = new StatContainer();
            stats.SetBase("ignite_duration", igniteDuration);
            stats.SetBase("chill_duration", chillDuration);
            stats.SetBase("skill_duration", skillDuration);

            return stats;
        }
    }
}