// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Marks an <see cref="SbModifier"/> subclass with a stable, designer-visible name. Consumers query the
    /// game's <c>NamedModifierRegistry</c> by this name to instantiate the modifier at runtime — talent trees,
    /// item affix rolls, enemy modifier loadouts, console commands, save/load by-name references.
    /// </summary>
    /// <remarks>
    /// Names should be unique across the loaded assembly graph; the registry asserts uniqueness on discovery.
    /// Convention is snake_case so console-typed names match the rest of the lib's stat naming.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class NamedModifierAttribute : Attribute {
        public string Name { get; }

        public NamedModifierAttribute(string name) {
            Name = name;
        }
    }
}
