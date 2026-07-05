// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class ConditionTests {
        private static readonly StatId Armor = new(1u);

        [Test]
        public void All_Empty_IsTrue() => Assert.IsTrue(new All().Met(new CircuitContext()));

        [Test]
        public void All_AllTrue_IsTrue() =>
                Assert.IsTrue(new All(new StubCondition(), new StubCondition()).Met(new CircuitContext()));

        [Test]
        public void All_ShortCircuitsOnFirstFalse() {
            var after = new StubCondition();

            Assert.IsFalse(new All(new StubCondition(false), after).Met(new CircuitContext()));
            Assert.AreEqual(0, after.EvaluationCount);
        }

        [Test]
        public void Any_Empty_IsFalse() => Assert.IsFalse(new Any().Met(new CircuitContext()));

        [Test]
        public void Any_ShortCircuitsOnFirstTrue() {
            var after = new StubCondition();

            Assert.IsTrue(new Any(new StubCondition(), after).Met(new CircuitContext()));
            Assert.AreEqual(0, after.EvaluationCount);
        }

        [Test]
        public void Any_AllFalse_IsFalse() =>
                Assert.IsFalse(new Any(new StubCondition(false), new StubCondition(false)).Met(new CircuitContext()));

        [Test]
        public void Not_Inverts() {
            Assert.IsFalse(new Not(new StubCondition()).Met(new CircuitContext()));
            Assert.IsTrue(new Not(new StubCondition(false)).Met(new CircuitContext()));
        }

        [Test]
        public void StatAtLeast_MetAtExactThreshold() {
            var modifiable = new Modifiable();
            modifiable.Stats.SetBase(Armor, 50f);
            var ctx = new CircuitContext { Subject = modifiable };

            Assert.IsTrue(new StatAtLeast(Armor, 50f).Met(ctx));
        }

        [Test]
        public void StatAtLeast_BelowThreshold_IsFalse() {
            var modifiable = new Modifiable();
            modifiable.Stats.SetBase(Armor, 50f);
            var ctx = new CircuitContext { Subject = modifiable };

            Assert.IsFalse(new StatAtLeast(Armor, 50.01f).Met(ctx));
        }

        [Test]
        public void StatAtLeast_NullSubject_IsFalse() =>
                Assert.IsFalse(new StatAtLeast(Armor, 0f).Met(new CircuitContext()));

        [Test]
        public void When_ConditionFalse_SkipsChild() {
            var leaf = new RecordingLeaf();

            new When(new StubCondition(false), leaf).Process(new CircuitContext());

            Assert.AreEqual(0, leaf.ProcessCount);
        }

        [Test]
        public void When_ConditionTrue_ProcessesChild() {
            var leaf = new RecordingLeaf();

            new When(new StubCondition(), leaf).Process(new CircuitContext());

            Assert.AreEqual(1, leaf.ProcessCount);
        }

        [Test]
        public void Sequence_ProcessesAllChildrenInOrder() {
            var log = new List<string>();

            new Sequence(
                new RecordingLeaf(log, "a"),
                new RecordingLeaf(log, "b"),
                new RecordingLeaf(log, "c")).Process(new CircuitContext());

            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, log);
        }
    }
}