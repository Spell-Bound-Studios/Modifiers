// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Hashing;
using Spellbound.Core.Registries;

namespace Spellbound.Modifiers {
    /// <summary>
    /// The identity machinery for events. The library names zero events; a host declares its own vocabulary as
    /// code constants — <c>public static readonly uint Hit = EventRegistry.Register("hit");</c> — and keys every
    /// raise / subscribe by the returned hash. Same seam as stats (<see cref="StatRegistry"/>) and packers
    /// (<c>[PackerId]</c>): the library supplies the registry, the game supplies the vocabulary.
    /// </summary>
    public static class EventRegistry {
        private static readonly HashRegistry<EventId> Registry = new();

        /// <summary>
        /// Register an event name and return its stable hash. Idempotent for the same name; throws when two
        /// distinct names collide on one hash. Declare events as static constants so the vocabulary is fixed at
        /// type-load.
        /// </summary>
        public static uint Register(string name) {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Event name must be non-empty.", nameof(name));

            var hash = StableHash.Fnv1A32(name);

            if (Registry.TryGet(hash, out var existing)) {
                if (existing.Name != name)
                    throw new InvalidOperationException(
                        $"Event hash collision: '{name}' and '{existing.Name}' both hash to {hash}. Rename one.");

                return hash;
            }

            Registry.Add(new EventId(hash, name));

            return hash;
        }

        /// <summary>
        /// The registered name for a hash; false if it is not a registered event. Used for readable diagnostics.
        /// </summary>
        public static bool TryGetName(uint hash, out string name) {
            if (Registry.TryGet(hash, out var entry)) {
                name = entry.Name;

                return true;
            }

            name = null;

            return false;
        }

        /// <summary>
        /// Every registered event — the source a designer-facing event picker would read.
        /// </summary>
        public static IReadOnlyList<EventId> All => Registry.All;
    }
}
