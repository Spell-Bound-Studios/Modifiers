// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Modifiers {
    /// <summary>
    /// A named, mutable insertion point in a circuit; sources grant nodes into it and revoke them by source id.
    /// Children run in ascending priority order, ties in grant order.
    /// </summary>
    public sealed class Stage : Node {
        private static readonly Comparison<Grant> GrantOrder = (a, b) =>
                a.Priority != b.Priority ? a.Priority.CompareTo(b.Priority) : a.Sequence.CompareTo(b.Sequence);

        private readonly List<Grant> _grants = new();
        private Node[] _children = Array.Empty<Node>();
        private bool _dirty;
        private int _sequence;

        public Stage(uint id) => Id = id;

        public uint Id { get; }

        public IReadOnlyList<Node> Children {
            get {
                if (_dirty)
                    Rebuild();

                return _children;
            }
        }

        public void Add(Node node, int priority = 0, uint sourceId = Contribution.Innate) {
            _grants.Add(new Grant(node, priority, sourceId, _sequence++));
            _dirty = true;
        }

        public int RemoveBySource(uint sourceId) {
            if (sourceId == Contribution.Innate)
                return 0;

            var removed = _grants.RemoveAll(g => g.SourceId == sourceId);

            if (removed > 0)
                _dirty = true;

            return removed;
        }

        public override void Process(CircuitContext ctx) {
            if (_dirty)
                Rebuild();

            for (var i = 0; i < _children.Length; i++)
                _children[i].Process(ctx);
        }

        private void Rebuild() {
            _grants.Sort(GrantOrder);

            if (_children.Length != _grants.Count)
                _children = new Node[_grants.Count];

            for (var i = 0; i < _grants.Count; i++)
                _children[i] = _grants[i].Node;

            _dirty = false;
        }

        private readonly struct Grant {
            public readonly Node Node;
            public readonly int Priority;
            public readonly uint SourceId;
            public readonly int Sequence;

            public Grant(Node node, int priority, uint sourceId, int sequence) {
                Node = node;
                Priority = priority;
                SourceId = sourceId;
                Sequence = sequence;
            }
        }
    }
}
