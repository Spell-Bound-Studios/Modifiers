// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    public class Modifiable {
        private readonly CircuitSet _circuits = new();
        private readonly CircuitContext _query = new();

        public StatBlock Stats { get; } = new();

        public virtual float GetValue(StatId stat) {
            _query.Subject = this;

            return Stats.GetValue(stat, _query);
        }

        public Circuit CircuitFor(uint identity) => _circuits.GetOrCreate(identity);

        public virtual void Run(uint identity, CircuitContext ctx) {
            if (!_circuits.TryGet(identity, out var circuit))
                return;

            var previous = ctx.Subject;
            ctx.Subject = this;
            circuit.Evaluate(ctx);
            ctx.Subject = previous;
        }

        public int RemoveSource(uint sourceId) => Stats.RemoveBySource(sourceId) + _circuits.RemoveBySource(sourceId);
    }
}
