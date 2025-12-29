// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Represents something that can receive modifiers (Skills, Characters, etc.)
    /// </summary>
    public interface IModifiable {
        /// <summary>
        /// Tags that determine which modifiers affect this entity.
        /// </summary>
        HashSet<int> Tags { get; }
    }
}