// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// The user-facing string-keyed API on top of <see cref="StatContainer"/>'s int-id internals. Hides the
    /// <see cref="StatRegistry"/> interning step behind <c>SetBase</c> / <c>GetValue</c> / <c>AddFlat</c> /
    /// <c>AddIncreased</c> / <c>AddMore</c>. Also threads an optional <see cref="StatDatabase"/> through
    /// <see cref="SetDatabase"/> so callers can resolve <see cref="StatDefinition"/> for pretty-printing via
    /// <see cref="GetFormattedValue"/>.
    /// </summary>
    public static class BehaviourExtensions {
        private static StatDatabase _database;

        public static void SetDatabase(StatDatabase database) => _database = database;

        #region Stat Definition Extensions

        public static string GetFormattedValue(this SbBehaviour container, string statName) {
            var value = container.GetValue(statName);

            if (_database == null)
                return value.ToString("F0");

            var definition = _database.GetDefinition(statName);

            return definition != null
                    ? definition.FormatValue(value)
                    : value.ToString("F0");
        }

        public static StatDefinition GetDefinition(this SbBehaviour container, string statName) =>
                _database?.GetDefinition(statName);

        #endregion

        #region Stat Container Extensions

        public static void SetBase(this SbBehaviour container, string statName, float value) =>
                container.SetBase(StatRegistry.Register(statName), value);

        public static float GetValue(this SbBehaviour container, string statName) =>
                container.GetValue(StatRegistry.Register(statName));

        public static void AddFlat(
            this SbBehaviour container, string statName, float value, string uniqueId = null) =>
                container.AddModifier(new StatModifier(
                    StatRegistry.Register(statName),
                    ModifierType.Flat,
                    value,
                    uniqueId
                ));

        public static void AddIncreased(
            this SbBehaviour container, string statName, float percent, string uniqueId = null) =>
                container.AddModifier(new StatModifier(
                    StatRegistry.Register(statName),
                    ModifierType.Increased,
                    percent,
                    uniqueId
                ));

        public static void AddMore(
            this SbBehaviour container, string statName, float percent, string uniqueId = null) =>
                container.AddModifier(new StatModifier(
                    StatRegistry.Register(statName),
                    ModifierType.More,
                    percent,
                    uniqueId
                ));

        #endregion
    }
}