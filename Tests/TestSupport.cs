// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spellbound.Modifiers.Tests {
    internal static class Definitions {
        public static ModifierDefinition Create(params ContributionSpecification[] contributions) {
            var definition = ScriptableObject.CreateInstance<ModifierDefinition>();
            SetField(definition, "contributions", new List<ContributionSpecification>(contributions));

            return definition;
        }

        public static ModifierGrantSet Grants(params ModifierGrant[] grants) {
            var set = new ModifierGrantSet();
            SetField(set, "grants", new List<ModifierGrant>(grants));

            return set;
        }

        public static NamedModifierGrant Named(ModifierDefinition definition) {
            var grant = new NamedModifierGrant();
            SetField(grant, "definition", definition);

            return grant;
        }

        public static FixedMagnitude Fixed(float value) {
            var magnitude = new FixedMagnitude();
            SetField(magnitude, "value", value);

            return magnitude;
        }

        public static RolledMagnitude Rolled(float min, float max, float step = 0f) {
            var magnitude = new RolledMagnitude();
            SetField(magnitude, "min", min);
            SetField(magnitude, "max", max);
            SetField(magnitude, "step", step);

            return magnitude;
        }

        public static DerivedMagnitude Derived(
            ScalarMagnitude amount, int perPoints, StatDefinition source, bool stepped = false,
            Perspective perspective = Perspective.Owner) {
            var magnitude = new DerivedMagnitude();
            SetField(magnitude, "amount", amount);
            SetField(magnitude, "perPoints", perPoints);
            SetField(magnitude, "stepped", stepped);
            SetField(magnitude, "source", source);
            SetField(magnitude, "perspective", perspective);

            return magnitude;
        }

        public static SingleStatContribution Single(StatDefinition stat, ContributionType type, Magnitude amount) {
            var contribution = new SingleStatContribution();
            SetField(contribution, "stat", stat);
            SetField(contribution, "contributionType", type);
            SetField(contribution, "amount", amount);

            return contribution;
        }

        public static StatBandContribution Band(
            StatDefinition lowStat, Magnitude lowAmount, StatDefinition highStat, Magnitude highAmount,
            ContributionType type = ContributionType.Flat, bool keepOrdered = true) {
            var contribution = new StatBandContribution();
            SetField(contribution, "lowStat", lowStat);
            SetField(contribution, "lowAmount", lowAmount);
            SetField(contribution, "highStat", highStat);
            SetField(contribution, "highAmount", highAmount);
            SetField(contribution, "contributionType", type);
            SetField(contribution, "keepOrdered", keepOrdered);

            return contribution;
        }

        public static MultiStatContribution Multi(
            ContributionType type, Magnitude amount, params StatDefinition[] stats) {
            var contribution = new MultiStatContribution();
            SetField(contribution, "stats", new List<StatDefinition>(stats));
            SetField(contribution, "contributionType", type);
            SetField(contribution, "amount", amount);

            return contribution;
        }

        private static void SetField(object target, string name, object value) =>
                target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);

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

        public static ContributionSpecification Range(
            StatDefinition stat, ContributionType type, float min, float max, float step = 0f,
            StatDefinition sourceStat = null) {
            ScalarMagnitude scalar = min == max ? Fixed(min) : Rolled(min, max, step);
            Magnitude magnitude = sourceStat != null ? Derived(scalar, 1, sourceStat) : scalar;

            return Single(stat, type, magnitude);
        }

        public static StatTemplate CreateTemplate(
            BaseStat[] bases, params ModifierDefinition[] innates) {
            var template = new StatTemplate();

            typeof(StatTemplate)
                    .GetField("baseStats", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(template, new List<BaseStat>(bases));

            typeof(StatTemplate)
                    .GetField("modifiers", BindingFlags.NonPublic | BindingFlags.Instance)
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