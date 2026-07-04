// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Modifiers {
    public class Circuit {
        private readonly Dictionary<uint, Stage> _stages = new();

        public Node Root { get; set; }

        public void Evaluate(CircuitContext ctx) => Root?.Process(ctx);

        public Stage DefineStage(uint id) {
            if (!_stages.TryGetValue(id, out var stage)) {
                stage = new Stage(id);
                _stages[id] = stage;
            }

            return stage;
        }

        public bool TryGetStage(uint id, out Stage stage) => _stages.TryGetValue(id, out stage);

        public int RemoveBySource(uint sourceId) {
            var removed = 0;

            foreach (var stage in _stages.Values)
                removed += stage.RemoveBySource(sourceId);

            return removed;
        }
    }
}
