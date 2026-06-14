// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Logging;

namespace Spellbound.Modifiers {
    /// <summary>
    /// An owner's live set of broadcastable <see cref="SbModifier"/> instances plus a generation stamp.
    /// Mutations are O(1) bookkeeping — no fan-out; satellites compare generations via
    /// <see cref="ModifierReceiver.Reconcile"/> and pull only when stale.
    /// </summary>
    public sealed class ModifierCache {
        private readonly List<SbModifier> _modifiers = new();

        /// <summary>
        /// Bumped on every mutation; a receiver whose recorded generation differs is stale.
        /// </summary>
        public int Generation { get; private set; }

        /// <summary>
        /// The live modifier set, in insertion order (application order matters — the first Override wins).
        /// </summary>
        public IReadOnlyList<SbModifier> Modifiers => _modifiers;

        public int Count => _modifiers.Count;

        public void Add(SbModifier modifier) {
            if (modifier == null) {
                Log.Error("Attempting to add a null modifier to the cache.");

                return;
            }

            _modifiers.Add(modifier);
            Generation++;
        }

        /// <summary>
        /// Removes one instance by reference identity. Returns true (and bumps the generation) when found.
        /// </summary>
        public bool Remove(SbModifier modifier) {
            if (modifier == null || !_modifiers.Remove(modifier))
                return false;

            Generation++;

            return true;
        }

        /// <summary>
        /// Adds every non-null entry; bumps the generation once when anything was added.
        /// </summary>
        public void AddRange(IReadOnlyList<SbModifier> modifiers) {
            if (modifiers == null) {
                Log.Error("Attempting to add a null modifier range to the cache.");

                return;
            }

            var added = 0;

            for (var i = 0; i < modifiers.Count; i++) {
                if (modifiers[i] == null)
                    continue;

                _modifiers.Add(modifiers[i]);
                added++;
            }

            if (added > 0)
                Generation++;
        }

        /// <summary>
        /// Removes every listed instance by reference identity; bumps the generation once when anything was
        /// removed. Returns the number removed.
        /// </summary>
        public int RemoveRange(IReadOnlyList<SbModifier> modifiers) {
            if (modifiers == null) {
                Log.Error("Attempting to remove a null modifier range from the cache.");

                return 0;
            }

            var removed = 0;

            for (var i = 0; i < modifiers.Count; i++) {
                if (modifiers[i] != null && _modifiers.Remove(modifiers[i]))
                    removed++;
            }

            if (removed > 0)
                Generation++;

            return removed;
        }

        public void Clear() {
            if (_modifiers.Count == 0)
                return;

            _modifiers.Clear();
            Generation++;
        }
    }
}
