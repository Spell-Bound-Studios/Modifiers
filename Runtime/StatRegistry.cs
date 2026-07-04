// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Registries;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Resolves stats by their stable GUID-derived hash. Auto-discovers every StatDefinition under a
    /// Resources/Stats folder; hand it a name to get the hash via the name index or a hash to get the
    /// definition.
    /// </summary>
    public static class StatRegistry {
        private const string ResourceFolder = "Stats";

        private static readonly HashRegistry<StatDefinition> Registry = new();
        private static readonly Dictionary<string, StatDefinition> NameIndex = new();
        private static bool _isLoaded;

        /// <summary>
        /// Every registered stat definition.
        /// </summary>
        public static IReadOnlyList<StatDefinition> All {
            get {
                EnsureLoaded();

                return Registry.All;
            }
        }

        #region Lifecycle

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForPlaySession() {
            Registry.Clear();
            NameIndex.Clear();
            _isLoaded = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void WarmUp() => EnsureLoaded();

        #endregion

        #region API

        /// <summary>
        /// The stable hash for a stat name, throwing if no such stat is registered. Call once, cache the uint,
        /// and key the hot paths by the hash so a per-frame read never touches the name index again.
        /// </summary>
        public static uint GetHash(string statName) {
            EnsureLoaded();

            if (!NameIndex.TryGetValue(statName, out var definition))
                throw new KeyNotFoundException(
                    $"Stat '{statName}' is not registered. Author a StatDefinition for it under Resources/{ResourceFolder}.");

            return definition.Hash;
        }

        /// <summary>
        /// The stable hash for a stat name; false if no such stat is registered.
        /// </summary>
        public static bool TryGetHash(string statName, out uint hash) {
            EnsureLoaded();

            if (NameIndex.TryGetValue(statName, out var definition)) {
                hash = definition.Hash;

                return true;
            }

            hash = 0u;

            return false;
        }

        /// <summary>
        /// True if a stat with this name is registered.
        /// </summary>
        public static bool IsRegistered(string statName) {
            EnsureLoaded();

            return NameIndex.ContainsKey(statName);
        }

        /// <summary>
        /// The definition for a stat hash, or null.
        /// </summary>
        public static StatDefinition GetDefinition(uint statHash) {
            EnsureLoaded();

            return Registry.TryGet(statHash, out var def) ? def : null;
        }

        /// <summary>
        /// The definition for a stat name, or null.
        /// </summary>
        public static StatDefinition GetDefinition(string statName) {
            EnsureLoaded();

            return NameIndex.TryGetValue(statName, out var definition) ? definition : null;
        }

        /// <summary>
        /// The name of the stat with this hash, or null.
        /// </summary>
        public static string GetName(uint statHash) => GetDefinition(statHash)?.StatName;

        /// <summary>
        /// The name of the stat with this hash; false if none is registered.
        /// </summary>
        public static bool TryGetName(uint statHash, out string statName) {
            statName = GetName(statHash);

            return statName != null;
        }

        #endregion

        #region Internal

        private static void EnsureLoaded() {
            if (_isLoaded)
                return;

            try {
                foreach (var definition in Resources.LoadAll<StatDefinition>(ResourceFolder)) {
                    if (Registry.Contains(definition.Hash))
                        throw new InvalidOperationException(
                            $"Stat hash collision: '{definition.StatName}' (asset '{definition.name}') collides with an " +
                            $"already-registered stat at hash {definition.Hash}. Regenerate one asset's GUID to resolve.");

                    if (!NameIndex.TryAdd(definition.StatName, definition))
                        throw new InvalidOperationException(
                            $"Duplicate stat name: '{definition.StatName}' (asset '{definition.name}') is already " +
                            "registered by another StatDefinition. Stat names must be unique — rename one.");

                    Registry.Add(definition);
                }
            }
            catch {
                Registry.Clear();
                NameIndex.Clear();

                throw;
            }

            _isLoaded = true;
        }

        #endregion
    }
}
