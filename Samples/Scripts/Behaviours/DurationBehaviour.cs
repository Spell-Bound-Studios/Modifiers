// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    [Serializable]
    public sealed class DurationBehaviour : SbBehaviour {
        [SerializeField] private float igniteDuration = 4f;
        [SerializeField] private float skillDuration = 5f;
        
        public float GetIgniteDuration() => Stats.GetValue("ignite_duration");
        
        public float GetSkillDuration() => Stats.GetValue("skill_duration");
        
        protected override StatContainer InitializeStats() {
            var stats = new StatContainer();
            stats.SetBase("ignite_duration", igniteDuration);
            stats.SetBase("skill_duration", skillDuration);
            return stats;
        }
    }
}