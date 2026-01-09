// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Stats {
    /// <summary>
    /// Implement this if you want something to be able to receive modifiers - it simply marks it as being modifiable.
    /// </summary>
    /// <example>
    /// Skills, Characters, Items - anything that modifiers can affect.
    /// </example>
    public interface ICanBeModified { }
}