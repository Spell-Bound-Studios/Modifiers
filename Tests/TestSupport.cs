// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spellbound.Modifiers.Tests {
    internal sealed class LogMute : IDisposable {
        private readonly bool _wasEnabled;

        public LogMute() {
            _wasEnabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            LogAssert.ignoreFailingMessages = true;
        }

        public void Dispose() {
            Debug.unityLogger.logEnabled = _wasEnabled;
            LogAssert.ignoreFailingMessages = false;
        }
    }

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
        public Modifiable LastOwner;

        public RecordingLeaf(List<string> log = null, string name = null) {
            _log = log;
            _name = name;
        }

        public override void Process(CircuitContext ctx) {
            ProcessCount++;
            LastSubject = ctx.Subject;
            LastOwner = ctx.Owner;
            _log?.Add(_name);
        }
    }
}
