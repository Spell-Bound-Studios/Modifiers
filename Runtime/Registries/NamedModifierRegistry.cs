// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Asset-based registry for <see cref="NamedModifier"/> SOs. Discovers every asset under
    /// <c>Resources/NamedModifiers/</c> at first query, indexes them by string key and by a
    /// stable uint hash of the key (FNV-1a 32-bit). Hash is deterministic — same key, same hash,
    /// every run — so save files and network payloads can pack a 4-byte id and resolve back to
    /// the asset on any machine.
    /// </summary>
    /// <remarks>
    /// Lazy-loaded on first query. Call <see cref="Refresh"/> to force a rescan (e.g. after
    /// runtime asset import). Collisions on key OR id are logged as errors at discovery time so
    /// they surface immediately rather than at first save-load.
    /// </remarks>
    public static class NamedModifierRegistry {
        private static Dictionary<string, NamedModifier> _byKey;
        private static Dictionary<uint, NamedModifier> _byId;

        public static NamedModifier GetByKey(string key) {
            EnsureLoaded();

            return _byKey.TryGetValue(key, out var asset) ? asset : null;
        }

        public static NamedModifier GetById(uint id) {
            EnsureLoaded();

            return _byId.TryGetValue(id, out var asset) ? asset : null;
        }

        public static IEnumerable<string> Keys {
            get {
                EnsureLoaded();

                return _byKey.Keys;
            }
        }

        public static IEnumerable<NamedModifier> All {
            get {
                EnsureLoaded();

                return _byKey.Values;
            }
        }

        /// <summary>
        /// Force a rescan of <c>Resources/NamedModifiers/</c>. The lazy path is normally enough;
        /// call this after editor-time asset changes or dynamic asset load.
        /// </summary>
        public static void Refresh() {
            _byKey = null;
            _byId = null;
            EnsureLoaded();
        }

        /// <summary>
        /// FNV-1a 32-bit hash of a key string. Stable, deterministic, identical across runs and
        /// machines. Use this to compute the uint id for any string key (e.g. when packing a
        /// modifier reference into a save or network frame before lookup).
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

            _byKey = new Dictionary<string, NamedModifier>();
            _byId = new Dictionary<uint, NamedModifier>();

            var assets = Resources.LoadAll<NamedModifier>("NamedModifiers");

            foreach (var asset in assets) {
                if (asset == null)
                    continue;

                if (string.IsNullOrEmpty(asset.Key)) {
                    Log.Error($"[NamedModifierRegistry] Asset '{asset.name}' has no Key; skipping.");

                    continue;
                }

                if (_byKey.TryGetValue(asset.Key, out var existing)) {
                    Log.Error(
                        $"[NamedModifierRegistry] Duplicate Key '{asset.Key}'. " +
                        $"Existing: {existing.name}; ignored: {asset.name}.");

                    continue;
                }

                var id = Hash(asset.Key);

                if (_byId.TryGetValue(id, out var collidingAsset)) {
                    Log.Error(
                        $"[NamedModifierRegistry] Hash collision: '{asset.Key}' (id={id}) collides with " +
                        $"existing '{collidingAsset.Key}'. Rename one of them. Ignoring '{asset.name}'.");

                    continue;
                }

                _byKey[asset.Key] = asset;
                _byId[id] = asset;
            }
        }
    }
}
