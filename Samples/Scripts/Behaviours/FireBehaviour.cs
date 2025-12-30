// Copyright 2025 Spellbound Studio Inc.

using System;

namespace Spellbound.Stats.Samples {
    public class FireBehaviour : IBehaviour {
        public string BehaviourType => "Fire";
        
        public event Action<object> OnIgnite;
        
        public bool CanIgnite { get; set; } = true;
        public float IgniteChance { get; set; } = 0f;
        
        public void TriggerIgnite(object target) {
            if (CanIgnite)
                OnIgnite?.Invoke(target);
        }
    }
}