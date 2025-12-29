// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Stats {
    /// <summary>
    /// Defines how a modifier's value is applied during stat calculation.
    /// This determines the mathematical operation, not what stat is being modified.
    /// </summary>
    public enum ModifierType : byte {
        /// <summary>
        /// Flat addition to base value. Applied first in calculation order.
        /// Example: "+10 to Strength"
        /// </summary>
        Flat = 0,

        /// <summary>
        /// Percentage increase, additive with other Increased modifiers.
        /// Example: "30% increased Physical Damage" + "20% increased Physical Damage" = 50% total
        /// </summary>
        Increased = 1,

        /// <summary>
        /// Percentage multiplier, multiplicative with other More modifiers.
        /// Example: "40% more Attack Speed" then "30% more Attack Speed" = 1.4 * 1.3 = 82% total
        /// </summary>
        More = 2,

        /// <summary>
        /// Overrides the final calculated value. Use sparingly for special cases.
        /// Example: "Set maximum Life to 1" (for certain unique mechanics)
        /// </summary>
        Override = 3
    }
}