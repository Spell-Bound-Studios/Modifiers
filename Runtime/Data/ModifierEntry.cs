// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored modifier that every instance of the containing preset gets at creation. Fed into the
    /// per-instance <see cref="SbBehaviour"/> alongside the base values from <see cref="StatBaseEntry"/>, so the
    /// computed totals reflect both intrinsic baseline AND preset-level modifiers (e.g., "every fir tree has
    /// +10% fire resistance baseline").
    /// </summary>
    /// <remarks>
    /// Runtime modifiers (gear, buffs, debuffs) layer on top of these via the same modifier system; the only
    /// difference is that template modifiers are baked in at instance creation, while runtime modifiers are
    /// added and removed dynamically.
    /// </remarks>
    [Serializable, InlineTemplate]
    public struct ModifierEntry {
        public StatDefinition stat;
        public ModifierType type;
        public float value;

        public override string ToString() {
            if (stat == null)
                return $"(no stat) {type}({value:G})";

            var name = string.IsNullOrEmpty(stat.DisplayName) ? stat.StatName : stat.DisplayName;

            return $"{name} {type}({value:G})";
        }
    }
}
