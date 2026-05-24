// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// Display-formatting extensions on <see cref="SbBehaviour"/> that depend on a host-supplied
    /// <see cref="StatDatabase"/>. Boot code calls <see cref="SetDatabase"/> once; UI code reads via
    /// <see cref="GetFormattedValue"/> / <see cref="GetDefinition"/>. Kept separate from
    /// <see cref="SbBehaviour"/> itself so the engine doesn't need to know about display formats or
    /// ScriptableObject assets — name-keyed stat math lives directly on <see cref="SbBehaviour"/>.
    /// </summary>
    public static class BehaviourExtensions {
        private static StatDatabase _database;

        public static void SetDatabase(StatDatabase database) => _database = database;

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
    }
}
