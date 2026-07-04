// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class CircuitTests {
        [Test]
        public void Evaluate_NoStages_DoesNotThrow() {
            Assert.DoesNotThrow(() => new Circuit().Evaluate(new CircuitContext()));
        }

        [Test]
        public void Evaluate_RunsStagesInAscendingOrder() {
            var circuit = new Circuit();
            var log = new List<string>();
            circuit.DefineStage(2u, 10).Add(new RecordingLeaf(log, "late"));
            circuit.DefineStage(1u, 0).Add(new RecordingLeaf(log, "early"));

            circuit.Evaluate(new CircuitContext());

            CollectionAssert.AreEqual(new[] { "early", "late" }, log);
        }

        [Test]
        public void Evaluate_TiedOrders_RunInDefinitionOrder() {
            var circuit = new Circuit();
            var log = new List<string>();
            circuit.DefineStage(1u, 5).Add(new RecordingLeaf(log, "first"));
            circuit.DefineStage(2u, 5).Add(new RecordingLeaf(log, "second"));

            circuit.Evaluate(new CircuitContext());

            CollectionAssert.AreEqual(new[] { "first", "second" }, log);
        }

        [Test]
        public void Evaluate_StageDefinedLater_SlotsBetweenExisting() {
            var circuit = new Circuit();
            var log = new List<string>();
            circuit.DefineStage(1u, 0).Add(new RecordingLeaf(log, "first"));
            circuit.DefineStage(3u, 20).Add(new RecordingLeaf(log, "third"));

            circuit.Evaluate(new CircuitContext());
            log.Clear();

            circuit.DefineStage(2u, 10).Add(new RecordingLeaf(log, "second"));
            circuit.Evaluate(new CircuitContext());

            CollectionAssert.AreEqual(new[] { "first", "second", "third" }, log);
        }

        [Test]
        public void DefineStage_SameId_ReturnsSameInstance() {
            var circuit = new Circuit();

            Assert.AreSame(circuit.DefineStage(3u, 0), circuit.DefineStage(3u, 0));
        }

        [Test]
        public void DefineStage_MismatchedOrder_KeepsOriginal() {
            var circuit = new Circuit();
            var log = new List<string>();
            var early = circuit.DefineStage(1u, 0);
            early.Add(new RecordingLeaf(log, "early"));
            circuit.DefineStage(2u, 10).Add(new RecordingLeaf(log, "late"));

            var redefined = circuit.DefineStage(1u, 99);
            circuit.Evaluate(new CircuitContext());

            Assert.AreSame(early, redefined);
            CollectionAssert.AreEqual(new[] { "early", "late" }, log);
        }

        [Test]
        public void Stages_ExposesOrderedView() {
            var circuit = new Circuit();
            var late = circuit.DefineStage(2u, 10);
            var early = circuit.DefineStage(1u, 0);

            Assert.AreEqual(2, circuit.Stages.Count);
            Assert.AreSame(early, circuit.Stages[0]);
            Assert.AreSame(late, circuit.Stages[1]);
        }

        [Test]
        public void TryGetStage_UnknownId_ReturnsFalse() {
            Assert.IsFalse(new Circuit().TryGetStage(3u, out _));
        }

        [Test]
        public void TryGetStage_DefinedId_ReturnsStage() {
            var circuit = new Circuit();
            var defined = circuit.DefineStage(3u, 0);

            Assert.IsTrue(circuit.TryGetStage(3u, out var found));
            Assert.AreSame(defined, found);
        }

        [Test]
        public void RemoveBySource_SweepsAllStages() {
            var circuit = new Circuit();
            const uint item = 55u;
            circuit.DefineStage(1u, 0).Add(new RecordingLeaf(), 0, item);
            circuit.DefineStage(2u, 10).Add(new RecordingLeaf(), 0, item);
            circuit.DefineStage(2u, 10).Add(new RecordingLeaf(), 0, 66u);

            Assert.AreEqual(2, circuit.RemoveBySource(item));
        }
    }
}
