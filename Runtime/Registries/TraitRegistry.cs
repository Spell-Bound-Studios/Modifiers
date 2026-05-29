// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Asset-based registry for <see cref="Trait"/> SOs. Scans <c>Resources/Traits/</c> on first
    /// query, indexes by string Key AND by stable uint hash (FNV-1a 32-bit) for compact save /
    /// network packing. Collisions on key or hash are logged at discovery so they surface
    /// immediately.
    /// </summary>
    public static class TraitRegistry {
        private static Dictionary<string, Trait> _byKey;
        private static Dictionary<uint, Trait> _byId;

        public static Trait GetByKey(string key) {
            EnsureLoaded();

            return _byKey.TryGetValue(key, out var asset) ? asset : null;
        }

        public static Trait GetById(uint id) {
            EnsureLoaded();

            return _byId.TryGetValue(id, out var asset) ? asset : null;
        }

        public static IEnumerable<string> Keys {
            get {
                EnsureLoaded();

                return _byKey.Keys;
            }
        }

        public static IEnumerable<Trait> All {
            get {
                EnsureLoaded();

                return _byKey.Values;
            }
        }

        public static void Refresh() {
            _byKey = null;
            _byId = null;
            EnsureLoaded();
        }

        /// <summary>
        /// FNV-1a 32-bit hash of a key string. Deterministic, stable across runs and machines.
        /// </summary>
        public static uint Hash(string key) {
            if (string.IsNullOrEmpty(key))
                return 0u;

            const uint offsetBasis = 2166136261u;
            const uint prime = 16777619u;
            var hash = offsetBasis;

            for (var i = 0; i < key.Length; i++) {
                hash ^= key[i];
                hash *= prime;
            }

            return hash;
        }

        private static void EnsureLoaded() {
            if (_byKey != null)
                return;

            _byKey = new Dictionary<string, Trait>();
            _byId = new Dictionary<uint, Trait>();

            var assets = Resources.LoadAll<Trait>("Traits");

            foreach (var asset in assets) {
                if (asset == null)
                    continue;

                if (string.IsNullOrEmpty(asset.Key)) {
                    Log.Error($"[TraitRegistry] Asset '{asset.name}' has no Key; skipping.");

                    continue;
                }

                if (_byKey.TryGetValue(asset.Key, out var existing)) {
                    Log.Error(
                        $"[TraitRegistry] Duplicate Key '{asset.Key}'. " +
                        $"Existing: {existing.name}; ignored: {asset.name}.");

                    continue;
                }

                var id = Hash(asset.Key);

                if (_byId.TryGetValue(id, out var collidingAsset)) {
                    Log.Error(
                        $"[TraitRegistry] Hash collision: '{asset.Key}' (id={id}) collides with " +
                        $"existing '{collidingAsset.Key}'. Rename one of them. Ignoring '{asset.name}'.");

                    continue;
                }

                _byKey[asset.Key] = asset;
                _byId[id] = asset;
            }
        }
    }
}
