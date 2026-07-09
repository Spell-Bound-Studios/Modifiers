// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Registries;
using UnityEngine;

namespace Spellbound.Modifiers {
    public static class ModifierRegistry {
        private const string ResourceFolder = "Modifiers";

        private static readonly HashRegistry<ModifierDefinition> Registry = new();
        private static readonly Dictionary<string, ModifierDefinition> NameIndex = new();
        private static bool _isLoaded;

        public static IReadOnlyList<ModifierDefinition> All {
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

        public static uint GetHash(string modifierName) {
            EnsureLoaded();

            if (!NameIndex.TryGetValue(modifierName, out var definition)) {
                throw new KeyNotFoundException(
                    $"Modifier '{modifierName}' is not registered. Author a ModifierDefinition for it under " +
                    $"Resources/{ResourceFolder}.");
            }

            return definition.Hash;
        }

        public static bool TryGetHash(string modifierName, out uint hash) {
            EnsureLoaded();

            if (NameIndex.TryGetValue(modifierName, out var definition)) {
                hash = definition.Hash;

                return true;
            }

            hash = 0u;

            return false;
        }

        public static bool IsRegistered(string modifierName) {
            EnsureLoaded();

            return NameIndex.ContainsKey(modifierName);
        }

        public static ModifierDefinition GetDefinition(uint modifierHash) {
            EnsureLoaded();

            return Registry.TryGet(modifierHash, out var definition) ? definition : null;
        }

        public static ModifierDefinition GetDefinition(string modifierName) {
            EnsureLoaded();

            return NameIndex.TryGetValue(modifierName, out var definition) ? definition : null;
        }

        public static string GetName(uint modifierHash) => GetDefinition(modifierHash)?.ModifierName;

        public static bool TryGetName(uint modifierHash, out string modifierName) {
            modifierName = GetName(modifierHash);

            return modifierName != null;
        }

        #endregion

        #region Internal

        private static void EnsureLoaded() {
            if (_isLoaded)
                return;

            try {
                foreach (var definition in Resources.LoadAll<ModifierDefinition>(ResourceFolder)) {
                    if (Registry.Contains(definition.Hash)) {
                        throw new InvalidOperationException(
                            $"Modifier hash collision: '{definition.ModifierName}' (asset '{definition.name}') collides " +
                            $"with an already-registered modifier at hash {definition.Hash}. Regenerate one asset's " +
                            "GUID to resolve.");
                    }

                    if (!NameIndex.TryAdd(definition.ModifierName, definition)) {
                        throw new InvalidOperationException(
                            $"Duplicate modifier name: '{definition.ModifierName}' (asset '{definition.name}') is " +
                            "already registered by another ModifierDefinition. Modifier names must be unique — rename one.");
                    }

                    var specs = definition.Contributions;
                    var rolledStats = new HashSet<uint>();

                    for (var i = 0; i < specs.Count; i++) {
                        if (specs[i] == null || !specs[i].IsValid) {
                            throw new InvalidOperationException(
                                $"Modifier '{definition.ModifierName}' (asset '{definition.name}') has a missing or " +
                                $"invalid contribution at index {i}.");
                        }

                        foreach (var (stat, _, amount) in specs[i].StatContributions)
                            RejectDuplicateRolledStat(rolledStats, stat, amount, definition);
                    }

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

        private static void RejectDuplicateRolledStat(
            HashSet<uint> seen, StatDefinition stat, Magnitude magnitude, ModifierDefinition definition) {
            if (stat == null || magnitude == null || !magnitude.Rolls)
                return;

            if (!seen.Add(stat.Hash)) {
                throw new InvalidOperationException(
                    $"Modifier '{definition.ModifierName}' (asset '{definition.name}') has two rolled contributions on " +
                    $"stat '{stat.StatName}'. Rolled values are keyed by stat and would collide — give each a distinct stat.");
            }
        }

        #endregion
    }
}