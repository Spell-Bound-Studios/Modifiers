// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Spellbound.Modifiers.Tests.Performance {
    public class CircuitPerformanceTests {
        private static readonly StatId Physical = new(1u);
        private static readonly StatId Fire = new(2u);
        private static readonly StatId Cold = new(3u);
        private static readonly StatId Lightning = new(4u);
        private static readonly StatId Armor = new(5u);
        private static readonly StatId FireRes = new(6u);
        private static readonly StatId ColdRes = new(7u);
        private static readonly StatId LightningRes = new(8u);

        private const uint TakeHit = 1u;

        private Modifiable _defender;
        private CircuitContext _ctx;
        private List<StatAndValue> _packet;
        private float _drained;

        [SetUp]
        public void SetUp() {
            _defender = new Modifiable();
            var stats = _defender.Stats;
            stats.SetBase(Armor, 10f);
            stats.SetBase(FireRes, 20f);
            stats.SetBase(ColdRes, 20f);
            stats.SetBase(LightningRes, 20f);

            var circuit = _defender.CircuitFor(TakeHit);
            var mitigate = circuit.DefineStage(1u, 0);
            var apply = circuit.DefineStage(2u, 10);

            mitigate.Add(new PercentReduceLeaf(Fire, FireRes));
            mitigate.Add(new PercentReduceLeaf(Cold, ColdRes));
            mitigate.Add(new PercentReduceLeaf(Lightning, LightningRes));
            mitigate.Add(new FlatReduceLeaf(Physical, Armor), 10);

            apply.Add(new DrainLeaf(amount => _drained += amount));

            _ctx = new CircuitContext();
            _packet = new List<StatAndValue>(4);

            RunOneHit();
        }

        private void RunOneHit() {
            _packet.Clear();
            _packet.Add(new StatAndValue(Physical, 40f));
            _packet.Add(new StatAndValue(Fire, 40f));
            _packet.Add(new StatAndValue(Cold, 40f));
            _packet.Add(new StatAndValue(Lightning, 40f));
            _ctx.Packet = _packet;
            _defender.Run(TakeHit, _ctx);
        }

        [Test, Performance]
        public void TakeHit_ThousandHitsThroughFullPipeline() =>
                Measure.Method(() => {
                            for (var hit = 0; hit < 1000; hit++)
                                RunOneHit();
                        })
                        .WarmupCount(5)
                        .MeasurementCount(20)
                        .IterationsPerMeasurement(1)
                        .GC()
                        .Run();

        [Test]
        public void TakeHit_SteadyState_DoesNotAllocate() =>
                Assert.That(() => RunOneHit(), Is.Not.AllocatingGCMemory());

        private sealed class PercentReduceLeaf : ModifierLeaf {
            private readonly StatId _damage;
            private readonly StatId _resistance;

            public PercentReduceLeaf(StatId damage, StatId resistance) {
                _damage = damage;
                _resistance = resistance;
            }

            public override void Process(CircuitContext ctx) {
                var packet = ctx.Packet;
                var resistance = ctx.Subject.GetValue(_resistance, ctx);

                if (resistance <= 0f)
                    return;

                var multiplier = 1f - resistance / 100f;

                for (var i = 0; i < packet.Count; i++) {
                    var entry = packet[i];

                    if (entry.statHash == _damage.Hash)
                        packet[i] = new StatAndValue(entry.statHash, entry.amount * multiplier);
                }
            }
        }

        private sealed class FlatReduceLeaf : ModifierLeaf {
            private readonly StatId _damage;
            private readonly StatId _reduction;

            public FlatReduceLeaf(StatId damage, StatId reduction) {
                _damage = damage;
                _reduction = reduction;
            }

            public override void Process(CircuitContext ctx) {
                var packet = ctx.Packet;
                var reduction = ctx.Subject.GetValue(_reduction, ctx);

                if (reduction <= 0f)
                    return;

                for (var i = 0; i < packet.Count; i++) {
                    var entry = packet[i];

                    if (entry.statHash == _damage.Hash)
                        packet[i] = new StatAndValue(entry.statHash, Math.Max(0f, entry.amount - reduction));
                }
            }
        }

        private sealed class DrainLeaf : ActionLeaf {
            private readonly Action<float> _drain;

            public DrainLeaf(Action<float> drain) {
                _drain = drain;
            }

            public override void Process(CircuitContext ctx) {
                var packet = ctx.Packet;
                var total = 0f;

                for (var i = 0; i < packet.Count; i++)
                    total += packet[i].amount;

                _drain(total);
            }
        }
    }
}