// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Logging;

namespace Spellbound.Modifiers {
    /// <summary>
    /// String-keyed lookup for <see cref="StatDefinition"/> assets — the counterpart to
    /// <see cref="StatRegistry"/> (which maps stat names to process-local integer ids). Lets
    /// runtime code resolve a stat name back to its full <see cref="StatDefinition"/> asset for
    /// display formatting, value parsing, etc. Used by <see cref="Affix.Unpack"/> to rebuild
    /// the asset reference from a packed stat name string.
    /// </summary>
    /// <remarks>
    /// Populated by <see cref="StatDefinition.Register"/> — each stat definition calls Register
    /// the first time it appears in any flow that needs an int id, which also threads the
    /// definition into this dictionary. <see cref="StatDatabase.RegisterAll"/> calls Register on
    /// every stat at boot, so the registry is fully populated before any pack/unpack work.
    /// The drop-in <see cref="StatDatabaseLoader"/> is the canonical eager-bootstrap path and
    /// populates both this registry and <see cref="StatRegistry"/> in one go.
    /// </remarks>
    public static class StatDefinitionRegistry {
        private static readonly Dictionary<string, StatDefinition> _byName = new();

        public static void Register(StatDefinition definition) {
            if (definition == null || string.IsNullOrEmpty(definition.StatName))
                return;

            if (_byName.TryGetValue(definition.StatName, out var existing) && existing != definition) {
                Log.Warn(
                    $"Duplicate stat name '{definition.StatName}'. " +
                    $"Existing asset '{existing.name}' is being replaced by '{definition.name}'.");
            }

            _byName[definition.StatName] = definition;
        }

        public static StatDefinition GetByName(string statName) {
            if (string.IsNullOrEmpty(statName))
                return null;

            return _byName.TryGetValue(statName, out var def) ? def : null;
        }

        public static void Clear() => _byName.Clear();
    }
}