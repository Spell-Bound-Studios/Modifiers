// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// Indicates that the implementer is unique.
    /// </summary>
    public interface IHasUniqueId {
        public string UniqueId { get; }
    }
}