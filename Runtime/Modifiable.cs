// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Logging;

namespace Spellbound.Modifiers {
    public class Modifiable {
        private readonly CircuitSet _circuits = new();
        private readonly CircuitContext _query = new();
        private Modifiable _parent;

        public StatBlock Stats { get; } = new();

        public Modifiable Parent {
            get => _parent;
            set {
                for (var m = value; m != null; m = m.Parent) {
                    if (m != this)
                        continue;

                    Log.Error("Rejected Modifiable.Parent assignment: it would create a cycle.");

                    return;
                }

                _parent = value;
            }
        }

        public virtual float GetValue(StatId stat) {
            _query.Subject = this;

            return GetValue(stat, _query);
        }

        public float GetValue(StatId stat, CircuitContext ctx) {
            var accumulator = new Accumulator();
            var baseInternal = 0;
            var hasBase = false;
            var previousOwner = ctx.Owner;

            for (var m = this; m != null; m = m.Parent) {
                ctx.Owner = m;
                m.Stats.Accumulate(stat, ctx, ref accumulator);

                if (!hasBase && m.Stats.TryGetBaseInternal(stat, out var b)) {
                    hasBase = true;
                    baseInternal = b;
                }
            }

            ctx.Owner = previousOwner;

            return StatSettings.ToExternal(accumulator.Resolve(baseInternal));
        }

        public Circuit CircuitFor(uint identity) => _circuits.GetOrCreate(identity);

        public virtual void Run(uint identity, CircuitContext ctx) {
            if (!_circuits.TryGet(identity, out var circuit))
                return;

            var previousSubject = ctx.Subject;
            var previousOwner = ctx.Owner;
            ctx.Subject = this;
            ctx.Owner = this;
            circuit.Evaluate(ctx);
            ctx.Subject = previousSubject;
            ctx.Owner = previousOwner;
        }

        public int RemoveSource(uint sourceId) => Stats.RemoveBySource(sourceId) + _circuits.RemoveBySource(sourceId);
    }
}
