// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Stats.Samples {
    public class ManaRecoupModifierDef : ModifierDefinition {
        public override string DisplayName => "Mana Recoup";
        public override float FixedValue => 3f;
        public override float MinValue => 1f;
        public override float MaxValue => 5f;
    
        public override IModifier CreateModifier(float value, int sourceId) {
            return new NumericModifier(
                modifierId: DisplayName.GetHashCode(),
                requiredTags: new HashSet<int>(),
                statModifier: new StatModifier(
                    StatRegistry.GetId("mana_recoup"),
                    ModifierType.Flat,
                    value,
                    sourceId
                )
            );
        }
    }
}