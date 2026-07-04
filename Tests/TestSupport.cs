// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Modifiers.Tests {
    internal sealed class StubCondition : Condition {
        public bool Result;
        public int EvaluationCount;

        public StubCondition(bool result = true) => Result = result;

        public override bool Met(CircuitContext ctx) {
            EvaluationCount++;

            return Result;
        }
    }

    internal sealed class RunOtherLeaf : Leaf {
        private readonly Modifiable _target;
        private readonly uint _identity;

        public RunOtherLeaf(Modifiable target, uint identity) {
            _target = target;
            _identity = identity;
        }

        public override void Process(CircuitContext ctx) => _target.Run(_identity, ctx);
    }

    internal sealed class RecordingLeaf : Leaf {
        private readonly List<string> _log;
        private readonly string _name;

        public int ProcessCount;
        public Modifiable LastSubject;

        public RecordingLeaf(List<string> log = null, string name = null) {
            _log = log;
            _name = name;
        }

        public override void Process(CircuitContext ctx) {
            ProcessCount++;
            LastSubject = ctx.Subject;
            _log?.Add(_name);
        }
    }
}
