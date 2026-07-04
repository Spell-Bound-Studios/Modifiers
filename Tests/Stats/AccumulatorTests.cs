// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class AccumulatorTests {
        private static int Internal(float value) => StatSettings.ToInternal(value);

        [Test]
        public void Resolve_NoModifiers_ReturnsBase() {
            var acc = new Accumulator();

            Assert.AreEqual(Internal(50f), acc.Resolve(Internal(50f)));
        }

        [Test]
        public void AddFlat_SumsOntoBase() {
            var acc = new Accumulator();
            acc.AddFlat(Internal(10f));
            acc.AddFlat(Internal(5f));

            Assert.AreEqual(Internal(65f), acc.Resolve(Internal(50f)));
        }

        [Test]
        public void AddIncreased_SumsAdditively() {
            var acc = new Accumulator();
            acc.AddIncreased(Internal(0.3f));
            acc.AddIncreased(Internal(0.2f));

            Assert.AreEqual(Internal(150f), acc.Resolve(Internal(100f)));
        }

        [Test]
        public void MultiplyMore_StacksMultiplicatively() {
            var acc = new Accumulator();
            acc.MultiplyMore(Internal(0.1f));
            acc.MultiplyMore(Internal(0.1f));

            Assert.AreEqual(Internal(121f), acc.Resolve(Internal(100f)));
        }

        [Test]
        public void AddIncreased_BelowMinusHundredPercent_ClampsToZero() {
            var acc = new Accumulator();
            acc.AddIncreased(Internal(-0.8f));
            acc.AddIncreased(Internal(-0.4f));

            Assert.AreEqual(0, acc.Resolve(Internal(100f)));
        }

        [Test]
        public void MultiplyMore_SingleBelowMinusHundredPercent_ClampsToZero() {
            var acc = new Accumulator();
            acc.MultiplyMore(Internal(-1.2f));

            Assert.AreEqual(0, acc.Resolve(Internal(100f)));
        }

        [Test]
        public void MultiplyMore_ExactlyHundredPercentLess_ResolvesToZero() {
            var acc = new Accumulator();
            acc.MultiplyMore(Internal(-1f));

            Assert.AreEqual(0, acc.Resolve(Internal(100f)));
        }

        [Test]
        public void MultiplyMore_LessStackedAfterFullImmunity_StaysZero() {
            var acc = new Accumulator();
            acc.MultiplyMore(Internal(-1f));
            acc.MultiplyMore(Internal(-0.2f));

            Assert.AreEqual(0, acc.Resolve(Internal(100f)));
        }

        [Test]
        public void MultiplyMore_TwoBelowMinusHundredPercent_StaysZero() {
            var acc = new Accumulator();
            acc.MultiplyMore(Internal(-1.2f));
            acc.MultiplyMore(Internal(-1.2f));

            Assert.AreEqual(0, acc.Resolve(Internal(100f)));
        }

        [Test]
        public void SetOverride_BeatsAllOtherModifiers() {
            var acc = new Accumulator();
            acc.AddFlat(Internal(500f));
            acc.AddIncreased(Internal(2f));
            acc.MultiplyMore(Internal(1f));
            acc.SetOverride(Internal(1f));

            Assert.AreEqual(Internal(1f), acc.Resolve(Internal(100f)));
        }

        [Test]
        public void SetOverride_LowestWinsRegardlessOfOrder() {
            var ascending = new Accumulator();
            ascending.SetOverride(Internal(1f));
            ascending.SetOverride(Internal(10f));

            var descending = new Accumulator();
            descending.SetOverride(Internal(10f));
            descending.SetOverride(Internal(1f));

            Assert.AreEqual(Internal(1f), ascending.Resolve(Internal(100f)));
            Assert.AreEqual(Internal(1f), descending.Resolve(Internal(100f)));
        }

        [Test]
        public void Apply_DispatchesByType() {
            var acc = new Accumulator();
            acc.Apply(ModifierType.Flat, Internal(10f));
            acc.Apply(ModifierType.Increased, Internal(0.5f));
            acc.Apply(ModifierType.More, Internal(0.1f));

            Assert.AreEqual(Internal(181.5f), acc.Resolve(Internal(100f)));
        }

        [Test]
        public void Apply_UnknownType_Throws() {
            var acc = new Accumulator();

            Assert.Throws<System.ArgumentOutOfRangeException>(() => acc.Apply((ModifierType)99, 0));
        }
    }
}
