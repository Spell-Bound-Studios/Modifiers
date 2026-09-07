// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class ModifierTableTests {
        [Test]
        public void Sample_WithoutReplacement_ReturnsDistinct() {
            var a = Definitions.Create();
            var b = Definitions.Create();
            var c = Definitions.Create();
            var table = Definitions.CreateTable((a, 1), (b, 1), (c, 1));

            var picked = table.Sample(3, new System.Random(7));

            Assert.AreEqual(3, picked.Count);
            CollectionAssert.AllItemsAreUnique(picked);
        }

        [Test]
        public void Sample_StopsWhenTableExhausted() {
            var table = Definitions.CreateTable((Definitions.Create(), 1), (Definitions.Create(), 1));

            Assert.AreEqual(2, table.Sample(5, new System.Random(7)).Count);
        }

        [Test]
        public void Sample_WithReplacement_CanRepeat() {
            var only = Definitions.Create();
            var table = Definitions.CreateTable((only, 1));

            var picked = table.Sample(3, new System.Random(7), true);

            Assert.AreEqual(3, picked.Count);

            foreach (var definition in picked)
                Assert.AreSame(only, definition);
        }

        [Test]
        public void Sample_SkipsZeroWeight() {
            var disabled = Definitions.Create();
            var active = Definitions.Create();
            var table = Definitions.CreateTable((disabled, 0), (active, 1));

            var picked = table.Sample(2, new System.Random(7));

            Assert.AreEqual(1, picked.Count);
            Assert.AreSame(active, picked[0]);
        }

        [Test]
        public void Sample_SameSeed_SameSequence() {
            var a = Definitions.Create();
            var b = Definitions.Create();
            var c = Definitions.Create();
            var d = Definitions.Create();
            var table = Definitions.CreateTable((a, 1), (b, 3), (c, 5), (d, 7));

            var first = table.Sample(3, new System.Random(99));
            var second = table.Sample(3, new System.Random(99));

            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void TryPick_NothingSlot_ReportsFalse() {
            var only = Definitions.Create();
            var table = Definitions.CreateTable(1, (only, 1));

            Assert.AreEqual(2, table.Table.TotalWeight);
            Assert.IsTrue(table.Table.TryPick(0, out var picked));
            Assert.AreSame(only, picked);
            Assert.IsFalse(table.Table.TryPick(1, out picked));
            Assert.IsNull(picked);
        }

        [Test]
        public void Sample_NothingSlot_ConsumesPicksWithoutAddingDefinitions() {
            var only = Definitions.Create();
            var table = Definitions.CreateTable(1, (only, 1));

            var picked = table.Sample(100, new System.Random(3), true);

            Assert.Greater(picked.Count, 0);
            Assert.Less(picked.Count, 100);

            foreach (var definition in picked)
                Assert.AreSame(only, definition);
        }
    }
}
