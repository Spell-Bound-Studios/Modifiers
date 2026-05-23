// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// The math half of a mitigation row: given an incoming damage magnitude and the owner's defensive
    /// stat value, return the reduced magnitude. Stateless by convention — per-row configuration is
    /// constructor-baked or absent, and modifiers wanting different math swap the strategy on the row
    /// rather than mutating the strategy instance.
    /// </summary>
    public interface IMitigationStrategy {
        float Apply(float damage, float defense);
    }
}
