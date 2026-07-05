// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Modifiers {
    public sealed class CircuitSet {
        private readonly Dictionary<uint, Circuit> _circuits = new();

        public Circuit GetOrCreate(uint identity) {
            if (!_circuits.TryGetValue(identity, out var circuit)) {
                circuit = new Circuit();
                _circuits[identity] = circuit;
            }

            return circuit;
        }

        public bool TryGet(uint identity, out Circuit circuit) => _circuits.TryGetValue(identity, out circuit);

        public int RemoveBySource(uint sourceId) {
            var removed = 0;

            foreach (var circuit in _circuits.Values)
                removed += circuit.RemoveBySource(sourceId);

            return removed;
        }
    }
}