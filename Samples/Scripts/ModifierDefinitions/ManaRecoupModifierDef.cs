/*// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Example modifier definition that grants mana recoup.
    /// Demonstrates a simple single-modifier definition with configurable roll range.
    /// </summary>
    public class ManaRecoupModifierDef : ModifierDefinition {
        public override string DisplayName => "Mana Recoup";
        
        protected override ModifierTemplate[] GetTemplates() {
            return new[] {
                new ModifierTemplate {
                    FixedValue = 5f,
                    MinValue = 1f,
                    MaxValue = 5f,
                    Description = "Mana Recoup",
                    CreateModifier = (value, sourceId) => new NumericModifier(
                        modifierId: DisplayName.GetHashCode(),
                        requiredTags: null,
                        statModifier: new StatModifier(
                            StatRegistry.GetId("mana_recoup"),
                            ModifierType.Flat,
                            value,
                            sourceId
                        )
                    )
                }
            };
        }
    }
}*/