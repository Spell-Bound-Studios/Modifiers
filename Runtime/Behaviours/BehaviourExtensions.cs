// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// Display-formatting extensions on <see cref="SbBehaviour"/>. Resolves a stat name to its
    /// <see cref="StatDefinition"/> through <see cref="StatRegistry"/> for formatting; falls back to a plain
    /// number when the stat has no definition.
    /// </summary>
    public static class BehaviourExtensions {
        public static string GetFormattedValue(this SbBehaviour container, string statName) {
            if (!StatRegistry.TryGetHash(statName, out var hash))
                return "0";

            var value = container.GetValue(hash);
            var definition = StatRegistry.GetDefinition(hash);

            return definition != null
                    ? definition.FormatValue(value)
                    : value.ToString("F0");
        }

        public static StatDefinition GetDefinition(this SbBehaviour container, string statName) =>
                StatRegistry.GetDefinition(statName);
    }
}
