// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Represents any modification to a skill or entity.
    /// Can be numerical (stat changes) or behavioral (on-kill effects, additional projectiles, etc.)
    /// </summary>
    public interface IModifier {
        /// <summary>
        /// Unique identifier for this modifier instance.
        /// </summary>
        int ModifierId { get; }

        /// <summary>
        /// Tags that determine what this modifier can affect.
        /// Example: [Fire, Spell] means this only applies to Fire Spells.
        /// </summary>
        HashSet<int> RequiredTags { get; }

        /// <summary>
        /// Apply this modifier to a target.
        /// </summary>
        void Apply(IModifiable target);

        /// <summary>
        /// Remove this modifier from a target.
        /// </summary>
        void Remove(IModifiable target);
    }
}