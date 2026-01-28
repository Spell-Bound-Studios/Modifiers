// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    [Serializable]
    public sealed class IncreasedDurationModifier : SbModifier {
        [SerializeField] private float increasedDurationPercent = 50f;
        
        public override void Apply(ICanBeModified target) {
            if (!TryGetBehaviour<DurationBehaviour>(target, out var duration)) 
                return;
            
            duration.Stats.AddIncreased("ignite_duration", increasedDurationPercent, UniqueId);
        }
        
        public override void Remove(ICanBeModified target) {
            if (!TryGetBehaviour<DurationBehaviour>(target, out var duration)) 
                return;
            
            duration.Stats.RemoveModifierByUniqueId(UniqueId);
        }
    }
}