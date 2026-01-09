// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    [Serializable]
    public class DurationBehaviour : SbBehaviour {
        public override string BehaviourType => "Duration";
        
        [SerializeField] private float igniteDuration = 4f;
        [SerializeField] private float skillDuration = 5f;
        
        public float GetIgniteDuration() => 
                Stats.GetValue(StatRegistry.GetId("ignite_duration"));
        
        public float GetSkillDuration() => 
                Stats.GetValue(StatRegistry.GetId("skill_duration"));
        
        protected override StatContainer InitializeStats() {
            var stats = new StatContainer();
            stats.SetBase(StatRegistry.Register("ignite_duration"), igniteDuration);
            stats.SetBase(StatRegistry.Register("skill_duration"), skillDuration);
            return stats;
        }
    }
}