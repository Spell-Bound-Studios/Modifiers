// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Modifiers {
    /// <summary>
    /// A <see cref="PipelineNode{TContext}"/> that runs child nodes, as a <see cref="PipelineGroupKind.Sequence"/>
    /// or <see cref="PipelineGroupKind.Parallel"/>. Both run the same pass; the <see cref="Kind"/> documents
    /// intent and drives how modifiers edit it. The edit methods act on direct children — reach a nested group
    /// first with <see cref="PipelineNode{TContext}.Find"/>, then edit that group.
    /// </summary>
    public sealed class GroupNode<TContext> : PipelineNode<TContext> where TContext : struct {
        private readonly List<PipelineNode<TContext>> _children;

        public PipelineGroupKind Kind { get; }
        public IReadOnlyList<PipelineNode<TContext>> Children => _children;

        public GroupNode(string id, PipelineGroupKind kind, IEnumerable<PipelineNode<TContext>> children) : base(id) {
            Kind = kind;
            _children = new List<PipelineNode<TContext>>(children);
        }

        public override void Process(in TContext context) {
            foreach (var child in _children)
                child.Process(in context);
        }

        public override PipelineNode<TContext> Find(string id) {
            if (Id == id)
                return this;

            foreach (var child in _children) {
                var found = child.Find(id);

                if (found != null)
                    return found;
            }

            return null;
        }

        public void Append(PipelineNode<TContext> node) => _children.Add(node);

        public void Prepend(PipelineNode<TContext> node) => _children.Insert(0, node);

        public bool InsertBefore(string id, PipelineNode<TContext> node) {
            for (var i = 0; i < _children.Count; i++) {
                if (_children[i].Id != id)
                    continue;

                _children.Insert(i, node);

                return true;
            }

            return false;
        }

        public bool Replace(string id, PipelineNode<TContext> node) {
            for (var i = 0; i < _children.Count; i++) {
                if (_children[i].Id != id)
                    continue;

                _children[i] = node;

                return true;
            }

            return false;
        }

        public bool Remove(string id) => _children.RemoveAll(child => child.Id == id) > 0;
    }
}
