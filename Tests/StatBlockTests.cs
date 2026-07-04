// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Spellbound.Modifiers.Tests {
    public class StatBlockTests {
        private static readonly StatId Armor = new(1u);
        private static readonly StatId Life = new(2u);

        private static float Value(StatBlock block, StatId stat) => block.GetValue(stat, new CircuitContext());

        [Test]
        public void GetBase_Unset_ReturnsZero() {
            Assert.AreEqual(0f, new StatBlock().GetBase(Armor));
        }

        [Test]
        public void SetBase_RoundTrips() {
            var block = new StatBlock();
            block.SetBase(Armor, 12.34f);

            Assert.AreEqual(12.34f, block.GetBase(Armor), 0.0001f);
            Assert.IsTrue(block.HasBase(Armor));
            Assert.IsFalse(block.HasBase(Life));
        }

        [Test]
        public void GetValue_BaseOnly() {
            var block = new StatBlock();
            block.SetBase(Armor, 100f);

            Assert.AreEqual(100f, Value(block, Armor));
        }

        [Test]
        public void GetValue_CombinesFlatIncreasedMore() {
            var block = new StatBlock();
            block.SetBase(Armor, 100f);
            block.AddModifier(Armor, ModifierType.Flat, 20f);
            block.AddModifier(Armor, ModifierType.Increased, 0.5f);
            block.AddModifier(Armor, ModifierType.More, 0.1f);

            Assert.AreEqual(198f, Value(block, Armor));
        }

        [Test]
        public void GetValue_ConditionalAppliedOnlyWhenMet() {
            var block = new StatBlock();
            var condition = new StubCondition(false);
            block.SetBase(Armor, 100f);
            block.AddModifier(Armor, ModifierType.Increased, 1f, 5u, condition);

            Assert.AreEqual(100f, Value(block, Armor));

            condition.Result = true;

            Assert.AreEqual(200f, Value(block, Armor));

            condition.Result = false;

            Assert.AreEqual(100f, Value(block, Armor));
        }

        [Test]
        public void RemoveBySource_RemovesAcrossStats_ReturnsCount() {
            var block = new StatBlock();
            const uint ring = 77u;
            block.SetBase(Armor, 100f);
            block.SetBase(Life, 50f);
            block.AddModifier(Armor, ModifierType.Flat, 10f, ring);
            block.AddModifier(Life, ModifierType.Flat, 25f, ring);
            block.AddModifier(Life, ModifierType.Flat, 5f, 88u);

            Assert.AreEqual(2, block.RemoveBySource(ring));
            Assert.AreEqual(100f, Value(block, Armor));
            Assert.AreEqual(55f, Value(block, Life));
        }

        [Test]
        public void RemoveBySource_UnknownSource_ReturnsZero() {
            Assert.AreEqual(0, new StatBlock().RemoveBySource(123u));
        }

        [Test]
        public void RemoveBySource_Innate_ReturnsZero() {
            var block = new StatBlock();
            block.AddModifier(Armor, ModifierType.Flat, 10f);

            LogAssert.ignoreFailingMessages = true;

            try {
                Assert.AreEqual(0, block.RemoveBySource(Contribution.Innate));
            }
            finally {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void Changed_FiresForEachMutation() {
            var block = new StatBlock();
            var received = new List<StatId>();
            block.Changed += received.Add;

            block.SetBase(Armor, 10f);
            block.AddModifier(Armor, ModifierType.Flat, 5f, 9u);
            block.RemoveBySource(9u);

            CollectionAssert.AreEqual(new[] { Armor, Armor, Armor }, received);
        }

        [Test]
        public void Changed_NotFiredForFailedRemoveOrClear() {
            var block = new StatBlock();
            block.SetBase(Armor, 10f);
            var count = 0;
            block.Changed += _ => count++;

            block.RemoveBySource(999u);
            block.Clear();

            Assert.AreEqual(0, count);
        }

        [Test]
        public void GetValue_RefreshesAfterRemove() {
            var block = new StatBlock();
            const uint buff = 42u;
            block.SetBase(Armor, 100f);
            block.AddModifier(Armor, ModifierType.More, 0.5f, buff);

            Assert.AreEqual(150f, Value(block, Armor));

            block.RemoveBySource(buff);

            Assert.AreEqual(100f, Value(block, Armor));
        }

        [Test]
        public void GetValue_ConditionalOverride_LowestWins() {
            var block = new StatBlock();
            var frozen = new StubCondition();
            block.SetBase(Armor, 100f);
            block.AddModifier(Armor, ModifierType.Override, 30f);
            block.AddModifier(Armor, ModifierType.Override, 0f, 5u, frozen);

            Assert.AreEqual(0f, Value(block, Armor));

            frozen.Result = false;

            Assert.AreEqual(30f, Value(block, Armor));
        }

        [Test]
        public void GetValue_SelfReferentialCondition_FallsBackWithoutOverflow() {
            var modifiable = new Modifiable();
            modifiable.Stats.SetBase(Armor, 100f);
            modifiable.Stats.AddModifier(Armor, ModifierType.Increased, 1f, 5u, new StatAtLeast(Armor, 50f));

            LogAssert.ignoreFailingMessages = true;

            try {
                Assert.AreEqual(200f, modifiable.GetValue(Armor));
            }
            finally {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void GetValue_IndirectConditionCycle_FallsBackWithoutOverflow() {
            var modifiable = new Modifiable();
            modifiable.Stats.SetBase(Armor, 100f);
            modifiable.Stats.SetBase(Life, 10f);
            modifiable.Stats.AddModifier(Armor, ModifierType.Flat, 50f, 1u, new StatAtLeast(Life, 5f));
            modifiable.Stats.AddModifier(Life, ModifierType.Flat, 50f, 2u, new StatAtLeast(Armor, 120f));

            LogAssert.ignoreFailingMessages = true;

            try {
                Assert.AreEqual(150f, modifiable.GetValue(Armor));
            }
            finally {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void Clear_ResetsEverything() {
            var block = new StatBlock();
            block.SetBase(Armor, 100f);
            block.AddModifier(Armor, ModifierType.Flat, 10f, 7u);
            block.Clear();

            Assert.AreEqual(0f, Value(block, Armor));
            Assert.IsFalse(block.HasBase(Armor));
            Assert.AreEqual(0, block.RemoveBySource(7u));
        }
    }
}
