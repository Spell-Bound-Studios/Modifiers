// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Editor.Tests {
    /// <summary>
    /// Pins the stat-math contract: PoE order, fixed-point exactness, and modifier add/remove
    /// semantics. Raw hashes throughout — the registry is never touched.
    /// </summary>
    public class SbBehaviourMathTests {
        private const uint StatA = 1u;
        private const uint StatB = 2u;
        private const uint Unset = 99u;

        private static StatModifierEntry Entry(ModifierType type, float value, string id = null) =>
                new(StatA, type, value, id);

        [Test]
        public void BaseOnly_ReturnsBase() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 100f);

            Assert.AreEqual(100f, b.GetValue(StatA));
        }

        [Test]
        public void FlatModifiers_SumAdditively() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 100f);
            b.AddModifier(Entry(ModifierType.Flat, 20f));
            b.AddModifier(Entry(ModifierType.Flat, 30f));

            Assert.AreEqual(150f, b.GetValue(StatA));
        }

        [Test]
        public void IncreasedModifiers_PoolAdditively_AppliedOnce() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 100f);
            b.AddModifier(Entry(ModifierType.Increased, 0.5f));
            b.AddModifier(Entry(ModifierType.Increased, 0.25f));

            Assert.AreEqual(175f, b.GetValue(StatA));
        }

        [Test]
        public void MoreModifiers_ChainMultiplicatively() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 100f);
            b.AddModifier(Entry(ModifierType.More, 0.4f));
            b.AddModifier(Entry(ModifierType.More, 0.3f));

            Assert.AreEqual(182f, b.GetValue(StatA));
        }

        [Test]
        public void FullPoEOrder_BaseFlatIncreasedMore() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 100f);
            b.AddModifier(Entry(ModifierType.Flat, 20f));
            b.AddModifier(Entry(ModifierType.Increased, 0.5f));
            b.AddModifier(Entry(ModifierType.More, 0.1f));

            Assert.AreEqual(198f, b.GetValue(StatA));
        }

        [Test]
        public void Override_IgnoresBaseFlatIncreasedMore() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 100f);
            b.AddModifier(Entry(ModifierType.Flat, 50f));
            b.AddModifier(Entry(ModifierType.Increased, 1f));
            b.AddModifier(Entry(ModifierType.More, 1f));
            b.AddModifier(Entry(ModifierType.Override, 42f));

            Assert.AreEqual(42f, b.GetValue(StatA));
        }

        [Test]
        public void TwoOverrides_FirstOneWins() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 100f);
            b.AddModifier(Entry(ModifierType.Override, 10f));
            b.AddModifier(Entry(ModifierType.Override, 20f));

            Assert.AreEqual(10f, b.GetValue(StatA));
        }

        [Test]
        public void NegativeFlatAndIncreased_BehaveAsSignedMath() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 100f);
            b.AddModifier(Entry(ModifierType.Flat, -30f));

            Assert.AreEqual(70f, b.GetValue(StatA));

            b.AddModifier(Entry(ModifierType.Increased, -0.5f));

            Assert.AreEqual(35f, b.GetValue(StatA));
        }

        [Test]
        public void FixedPoint_FlatSumIsExact() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 0.1f);
            b.AddModifier(Entry(ModifierType.Flat, 0.2f));

            Assert.AreEqual(0.3f, b.GetValue(StatA));
        }

        [Test]
        public void FixedPoint_IntermediateDivisionTruncates() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 0.0001f);
            b.AddModifier(Entry(ModifierType.Increased, 0.5f));

            Assert.AreEqual(0.0001f, b.GetValue(StatA));
        }

        [Test]
        public void ModifierOnStatWithNoBase_ComputesFromZero() {
            var b = new SbBehaviour();
            b.AddModifier(Entry(ModifierType.Flat, 50f));

            Assert.AreEqual(50f, b.GetValue(StatA));
        }

        [Test]
        public void UnsetStat_ReadsZero() {
            var b = new SbBehaviour();

            Assert.AreEqual(0f, b.GetValue(Unset));
        }

        [Test]
        public void MutationAfterRead_ReflectsOnNextRead() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 100f);

            Assert.AreEqual(100f, b.GetValue(StatA));

            b.AddModifier(Entry(ModifierType.Flat, 10f));

            Assert.AreEqual(110f, b.GetValue(StatA));
        }

        [Test]
        public void RemoveByUniqueId_RestoresExactValue_AcrossModifierTypes() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 100f);
            b.AddModifier(Entry(ModifierType.Flat, 10f, "x"));
            b.AddModifier(Entry(ModifierType.Increased, 0.5f, "x"));

            Assert.AreNotEqual(100f, b.GetValue(StatA));
            Assert.AreEqual(2, b.RemoveModifierByUniqueId("x"));
            Assert.AreEqual(100f, b.GetValue(StatA));
        }

        [Test]
        public void RemoveOneId_LeavesOtherApplied() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 100f);
            b.AddModifier(Entry(ModifierType.Flat, 10f, "a"));
            b.AddModifier(Entry(ModifierType.Flat, 5f, "b"));

            b.RemoveModifierByUniqueId("a");

            Assert.AreEqual(105f, b.GetValue(StatA));
        }

        [Test]
        public void RemoveAbsentId_ReturnsZero_DoesNotDirty() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 100f);

            var fires = 0;
            b.OnStatsDirty += () => fires++;

            Assert.AreEqual(0, b.RemoveModifierByUniqueId("nope"));
            Assert.AreEqual(0, fires);
        }

        [Test]
        public void DistinctStats_DoNotBleed() {
            var b = new SbBehaviour();
            b.SetBase(StatA, 100f);
            b.SetBase(StatB, 50f);
            b.AddModifier(new StatModifierEntry(StatA, ModifierType.Flat, 10f));

            Assert.AreEqual(110f, b.GetValue(StatA));
            Assert.AreEqual(50f, b.GetValue(StatB));
        }
    }
}
