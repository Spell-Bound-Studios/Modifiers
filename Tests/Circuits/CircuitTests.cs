// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class CircuitTests {
        [Test]
        public void Evaluate_NullRoot_DoesNotThrow() {
            Assert.DoesNotThrow(() => new Circuit().Evaluate(new CircuitContext()));
        }

        [Test]
        public void Evaluate_ProcessesRoot() {
            var circuit = new Circuit();
            var leaf = new RecordingLeaf();
            circuit.Root = leaf;

            circuit.Evaluate(new CircuitContext());

            Assert.AreEqual(1, leaf.ProcessCount);
        }

        [Test]
        public void DefineStage_SameId_ReturnsSameInstance() {
            var circuit = new Circuit();

            Assert.AreSame(circuit.DefineStage(3u), circuit.DefineStage(3u));
        }

        [Test]
        public void TryGetStage_UnknownId_ReturnsFalse() {
            Assert.IsFalse(new Circuit().TryGetStage(3u, out _));
        }

        [Test]
        public void TryGetStage_DefinedId_ReturnsStage() {
            var circuit = new Circuit();
            var defined = circuit.DefineStage(3u);

            Assert.IsTrue(circuit.TryGetStage(3u, out var found));
            Assert.AreSame(defined, found);
        }

        [Test]
        public void RemoveBySource_SweepsAllStages() {
            var circuit = new Circuit();
            const uint item = 55u;
            circuit.DefineStage(1u).Add(new RecordingLeaf(), 0, item);
            circuit.DefineStage(2u).Add(new RecordingLeaf(), 0, item);
            circuit.DefineStage(2u).Add(new RecordingLeaf(), 0, 66u);

            Assert.AreEqual(2, circuit.RemoveBySource(item));
        }
    }
}
