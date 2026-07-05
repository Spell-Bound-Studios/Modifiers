// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class ModifierPoolTests {
        [Test]
        public void Sample_WithoutReplacement_ReturnsDistinct() {
            var a = Definitions.Create();
            var b = Definitions.Create();
            var c = Definitions.Create();
            var pool = Definitions.CreatePool((a, 1), (b, 1), (c, 1));

            var picked = pool.Sample(3, new System.Random(7));

            Assert.AreEqual(3, picked.Count);
            CollectionAssert.AllItemsAreUnique(picked);
        }

        [Test]
        public void Sample_StopsWhenPoolExhausted() {
            var pool = Definitions.CreatePool((Definitions.Create(), 1), (Definitions.Create(), 1));

            Assert.AreEqual(2, pool.Sample(5, new System.Random(7)).Count);
        }

        [Test]
        public void Sample_WithReplacement_CanRepeat() {
            var only = Definitions.Create();
            var pool = Definitions.CreatePool((only, 1));

            var picked = pool.Sample(3, new System.Random(7), withReplacement: true);

            Assert.AreEqual(3, picked.Count);

            foreach (var definition in picked)
                Assert.AreSame(only, definition);
        }

        [Test]
        public void Sample_SkipsZeroWeight() {
            var disabled = Definitions.Create();
            var active = Definitions.Create();
            var pool = Definitions.CreatePool((disabled, 0), (active, 1));

            var picked = pool.Sample(2, new System.Random(7));

            Assert.AreEqual(1, picked.Count);
            Assert.AreSame(active, picked[0]);
        }

        [Test]
        public void Sample_SameSeed_SameSequence() {
            var a = Definitions.Create();
            var b = Definitions.Create();
            var c = Definitions.Create();
            var d = Definitions.Create();
            var pool = Definitions.CreatePool((a, 1), (b, 3), (c, 5), (d, 7));

            var first = pool.Sample(3, new System.Random(99));
            var second = pool.Sample(3, new System.Random(99));

            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void Roll_AssignsNonZeroSourceIds() {
            var pool = Definitions.CreatePool(
                (Definitions.Create(), 1), (Definitions.Create(), 1), (Definitions.Create(), 1));

            var rolled = pool.Roll(3, new System.Random(7));

            Assert.AreEqual(3, rolled.Count);

            foreach (var modifier in rolled)
                Assert.AreNotEqual(0u, modifier.sourceId);
        }
    }
}
