// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Logging;

namespace Spellbound.Modifiers {
    /// <summary>
    /// A satellite's record of which owner modifiers it has injected into its target and the cache
    /// generation it last synced at. <see cref="Reconcile"/> is a full resync: sweep the previously
    /// injected ids, apply the cache's current set, record the new ids + generation. The consumer decides
    /// when to call it; the generation comparison is the O(1) clean-path check.
    /// </summary>
    public sealed class ModifierReceiver {
        private readonly ICanBeModified _target;
        private readonly List<string> _injectedIds = new();
        private ModifierCache _lastSource;
        private int _lastSyncedGeneration = -1;

        public ModifierReceiver(ICanBeModified target) {
            if (target == null)
                Log.Error("ModifierReceiver constructed with a null target; every Reconcile will no-op.");

            _target = target;
        }

        /// <summary>
        /// Number of modifier ids currently injected into the target.
        /// </summary>
        public int InjectedCount => _injectedIds.Count;

        /// <summary>
        /// True when this receiver already reflects the cache's current generation.
        /// </summary>
        public bool IsSyncedWith(ModifierCache cache) =>
                cache != null
                && ReferenceEquals(_lastSource, cache)
                && _lastSyncedGeneration == cache.Generation;

        /// <summary>
        /// Full resync against the cache when stale: sweep the previously injected ids, apply the cache's
        /// current modifiers in order, record their ids and the generation. Returns true when a resync ran.
        /// </summary>
        public bool Reconcile(ModifierCache cache) {
            if (cache == null) {
                Log.Error("Attempting to reconcile against a null cache.");

                return false;
            }

            if (_target == null || IsSyncedWith(cache))
                return false;

            Sweep();

            for (var i = 0; i < cache.Modifiers.Count; i++) {
                var modifier = cache.Modifiers[i];

                modifier.Apply(_target);
                _injectedIds.Add(modifier.UniqueId);
            }

            _lastSource = cache;
            _lastSyncedGeneration = cache.Generation;

            return true;
        }

        /// <summary>
        /// Forces the next <see cref="Reconcile"/> to run even at a matching generation — call when the
        /// target's own behaviour set changed.
        /// </summary>
        public void Invalidate() => _lastSyncedGeneration = -1;

        /// <summary>
        /// Sweeps everything this receiver injected and forgets its source; the next Reconcile starts fresh.
        /// </summary>
        public void Detach() {
            Sweep();
            _lastSource = null;
            _lastSyncedGeneration = -1;
        }

        private void Sweep() {
            if (_target is IHasBehaviours hasBehaviours) {
                foreach (var id in _injectedIds)
                    hasBehaviours.Behaviours.RemoveModifierByUniqueId(id);
            }

            _injectedIds.Clear();
        }
    }
}
