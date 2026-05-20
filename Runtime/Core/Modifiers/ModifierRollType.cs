// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// Distinguishes a fixed-value modifier roll (one number) from a ranged roll (rolled within a min/max
    /// when the modifier is generated — the PoE affix pattern).
    /// </summary>
    /// <remarks>
    /// REDUNDANT as of this audit — the enum is defined but no type in the library references it.
    /// <see cref="StatModifier"/> and <see cref="StatModifierTemplate"/> both store a single <c>float Value</c>
    /// with no roll metadata. Either delete the file or wire it into a roll-resolution path (e.g. add a
    /// <c>min</c>/<c>max</c> pair to <see cref="StatModifierTemplate"/> and choose a value at instance creation
    /// based on this enum). Leaving it as-is gives external readers a false signal that ranged rolls are
    /// supported when they aren't.
    /// </remarks>
    public enum ModifierRollType {
        Fixed,
        Range
    }
}