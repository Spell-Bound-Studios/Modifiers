// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spellbound.Modifiers.Tests {
    internal static class Definitions {
        public static ModifierDefinition Create(params ContributionRange[] contributions) {
            var definition = ScriptableObject.CreateInstance<ModifierDefinition>();

            typeof(ModifierDefinition)
                    .GetField("contributions", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(definition, new List<ContributionRange>(contributions));

            return definition;
        }

        public static ModifierPool CreatePool(params (ModifierDefinition candidate, int weight)[] entries) {
            var pool = ScriptableObject.CreateInstance<ModifierPool>();
            var list = new List<WeightedEntry<ModifierDefinition>>();

            foreach (var (candidate, weight) in entries)
                list.Add(new WeightedEntry<ModifierDefinition> { candidate = candidate, weight = weight });

            typeof(WeightedPool<ModifierDefinition>)
                    .GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(pool, list);

            return pool;
        }

        public static ContributionRange Range(
            StatDefinition stat, ContributionType type, float min, float max, float step = 0f,
            StatDefinition sourceStat = null) =>
                new() { stat = stat, type = type, min = min, max = max, step = step, sourceStat = sourceStat };

        public static StatTemplate CreateTemplate(
            BaseStat[] bases, params ModifierDefinition[] innates) {
            var template = new StatTemplate();

            typeof(StatTemplate)
                    .GetField("baseStats", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(template, new List<BaseStat>(bases));

            typeof(StatTemplate)
                    .GetField("innateModifiers", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(template, new List<ModifierDefinition>(innates));

            return template;
        }
    }

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

        public StubCondition(bool result = true) {
            Result = result;
        }

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