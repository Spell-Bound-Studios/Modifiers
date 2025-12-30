// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Stats.Samples {
    public class DamageConversionBehaviour : IBehaviour {
        public string BehaviourType => "DamageConversion";
        
        public string FromDamageType { get; set; }
        public string ToDamageType { get; set; }
        public float ConversionPercent { get; set; }
    }
}