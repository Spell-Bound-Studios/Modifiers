// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class CircuitSetTests {
        [Test]
        public void GetOrCreate_SameIdentity_ReturnsSameCircuit() {
            var set = new CircuitSet();

            Assert.AreSame(set.GetOrCreate(7u), set.GetOrCreate(7u));
        }

        [Test]
        public void TryGet_UnknownIdentity_ReturnsFalse() {
            Assert.IsFalse(new CircuitSet().TryGet(7u, out _));
        }

        [Test]
        public void TryGet_AfterGetOrCreate_ReturnsCircuit() {
            var set = new CircuitSet();
            var created = set.GetOrCreate(7u);

            Assert.IsTrue(set.TryGet(7u, out var found));
            Assert.AreSame(created, found);
        }

        [Test]
        public void RemoveBySource_SweepsAllCircuits() {
            var set = new CircuitSet();
            const uint item = 55u;
            set.GetOrCreate(1u).DefineStage(1u, 0).Add(new RecordingLeaf(), 0, item);
            set.GetOrCreate(2u).DefineStage(1u, 0).Add(new RecordingLeaf(), 0, item);

            Assert.AreEqual(2, set.RemoveBySource(item));
        }
    }
}
