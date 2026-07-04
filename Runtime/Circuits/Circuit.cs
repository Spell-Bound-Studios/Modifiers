// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;

namespace Spellbound.Modifiers {
    public class Circuit {
        private static readonly Comparison<Entry> EntryOrder = (a, b) =>
                a.Order != b.Order ? a.Order.CompareTo(b.Order) : a.Sequence.CompareTo(b.Sequence);

        private readonly List<Entry> _entries = new();
        private readonly Dictionary<uint, Stage> _byId = new();
        private Stage[] _ordered = Array.Empty<Stage>();
        private bool _dirty;
        private int _sequence;

        public IReadOnlyList<Stage> Stages {
            get {
                if (_dirty)
                    Rebuild();

                return _ordered;
            }
        }

        public Stage DefineStage(uint id, int order) {
            if (_byId.TryGetValue(id, out var existing)) {
                var definedOrder = OrderOf(existing);

                if (definedOrder != order)
                    Log.Warn($"DefineStage: stage {id} is already defined at order {definedOrder}; " +
                             $"requested order {order} ignored.");

                return existing;
            }

            var stage = new Stage(id);
            _byId[id] = stage;
            _entries.Add(new Entry(stage, order, _sequence++));
            _dirty = true;

            return stage;
        }

        public bool TryGetStage(uint id, out Stage stage) => _byId.TryGetValue(id, out stage);

        public void Evaluate(CircuitContext ctx) {
            if (_dirty)
                Rebuild();

            for (var i = 0; i < _ordered.Length; i++)
                _ordered[i].Process(ctx);
        }

        public int RemoveBySource(uint sourceId) {
            var removed = 0;

            for (var i = 0; i < _entries.Count; i++)
                removed += _entries[i].Stage.RemoveBySource(sourceId);

            return removed;
        }

        private int OrderOf(Stage stage) {
            for (var i = 0; i < _entries.Count; i++) {
                if (_entries[i].Stage == stage)
                    return _entries[i].Order;
            }

            return 0;
        }

        private void Rebuild() {
            _entries.Sort(EntryOrder);

            if (_ordered.Length != _entries.Count)
                _ordered = new Stage[_entries.Count];

            for (var i = 0; i < _entries.Count; i++)
                _ordered[i] = _entries[i].Stage;

            _dirty = false;
        }

        private readonly struct Entry {
            public readonly Stage Stage;
            public readonly int Order;
            public readonly int Sequence;

            public Entry(Stage stage, int order, int sequence) {
                Stage = stage;
                Order = order;
                Sequence = sequence;
            }
        }
    }
}
