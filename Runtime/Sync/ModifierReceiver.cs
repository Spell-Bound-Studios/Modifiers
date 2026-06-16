// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Logging;

namespace Spellbound.Modifiers {
    /// <summary>
    /// A satellite's link to an owner's <see cref="ModifierCache"/>: it holds its own clones of the owner's
    /// modifiers, applied to the target, plus the cache generation it last synced at. <see cref="Reconcile"/>
    /// is a full resync — tear down the previously injected clones through each modifier's own
    /// <see cref="SbModifier.Remove"/>, then clone the cache's current set and <see cref="SbModifier.Apply"/>
    /// each to the target, recording the new clones + generation. Cloning per satellite keeps stateful
    /// modifiers isolated (each target gets its own instance); undoing through the modifier means anything —
    /// a stat change, a state swap, a behaviour swap — round-trips, not just stat entries. The consumer
    /// decides when to call <see cref="Reconcile"/>; the generation comparison is the O(1) clean-path check.
    /// Target-agnostic by construction: it touches only <see cref="ICanBeModified"/>, so any modifiable thing
    /// can be a satellite of any owner.
    /// </summary>
    public sealed class ModifierReceiver {
        private readonly ICanBeModified _target;
        private readonly List<SbModifier> _injected = new();
        private ModifierCache _lastSource;
        private int _lastSyncedGeneration = -1;

        public ModifierReceiver(ICanBeModified target) {
            if (target == null)
                Log.Error("ModifierReceiver constructed with a null target; every Reconcile will no-op.");

            _target = target;
        }

        /// <summary>
        /// Number of modifier clones currently applied to the target.
        /// </summary>
        public int InjectedCount => _injected.Count;

        /// <summary>
        /// True when this receiver already reflects the cache's current generation.
        /// </summary>
        public bool IsSyncedWith(ModifierCache cache) =>
                cache != null
                && ReferenceEquals(_lastSource, cache)
                && _lastSyncedGeneration == cache.Generation;

        /// <summary>
        /// Full resync against the cache when stale: tear down the previously injected clones, clone the
        /// cache's current modifiers and apply each to the target, record the clones and the generation.
        /// Returns true when a resync ran.
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
                var clone = (SbModifier)cache.Modifiers[i].Clone();

                clone.Apply(_target);
                _injected.Add(clone);
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
        /// Tears down everything this receiver injected and forgets its source; the next Reconcile starts fresh.
        /// </summary>
        public void Detach() {
            Sweep();
            _lastSource = null;
            _lastSyncedGeneration = -1;
        }

        private void Sweep() {
            foreach (var modifier in _injected)
                modifier.Remove(_target);

            _injected.Clear();
        }
    }
}
