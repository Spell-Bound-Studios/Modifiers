// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// Contract for "this instance carries a stable string identity." Used by the stat system so a modifier
    /// added now can be removed later by the exact same instance — see
    /// <see cref="StatContainer.RemoveModifierByUniqueId"/>. Power users who can't (or won't) inherit
    /// <see cref="SbModifier"/> implement this alongside <see cref="IModifier"/> directly.
    /// </summary>
    public interface IHasUniqueId {
        public string UniqueId { get; }
    }
}