// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;
using UnityEngine;

namespace Spellbound.Modifiers.Tests {
    public class ProgressionCurveTests {
        private static ProgressionCurve Curve(params float[] costs) {
            var curve = ScriptableObject.CreateInstance<ProgressionCurve>();
            curve.Define(costs);

            return curve;
        }

        [Test]
        public void StepFor_BelowFirstThreshold_IsZero() => Assert.AreEqual(0, Curve(100f, 150f).StepFor(99.9f));

        [Test]
        public void StepFor_AtExactThreshold_Advances() => Assert.AreEqual(1, Curve(100f, 150f).StepFor(100f));

        [Test]
        public void StepFor_BeyondLastThreshold_CapsAtStepCount() =>
                Assert.AreEqual(2, Curve(100f, 150f).StepFor(9999f));

        [Test]
        public void StepFor_NegativeTotal_IsZero() => Assert.AreEqual(0, Curve(100f).StepFor(-5f));

        [Test]
        public void ThresholdFor_IsCumulative() {
            var curve = Curve(100f, 150f);

            Assert.AreEqual(0f, curve.ThresholdFor(0));
            Assert.AreEqual(100f, curve.ThresholdFor(1));
            Assert.AreEqual(250f, curve.ThresholdFor(2));
            Assert.AreEqual(float.PositiveInfinity, curve.ThresholdFor(3));
        }

        [Test]
        public void CostFor_InRange_ReturnsCost() {
            var curve = Curve(100f, 150f);

            Assert.AreEqual(100f, curve.CostFor(0));
            Assert.AreEqual(150f, curve.CostFor(1));
        }

        [Test]
        public void CostFor_OutOfRange_IsUnreachable() {
            Assert.AreEqual(float.PositiveInfinity, Curve(100f).CostFor(1));
            Assert.AreEqual(float.PositiveInfinity, Curve(100f).CostFor(-1));
        }

        [Test]
        public void NegativeCost_TreatedAsZero() {
            var curve = Curve(100f, -50f, 100f);

            using (new LogMute()) {
                Assert.AreEqual(100f, curve.ThresholdFor(2));
                Assert.AreEqual(2, curve.StepFor(100f));
                Assert.AreEqual(200f, curve.ThresholdFor(3));
            }
        }

        [Test]
        public void EmptyCurve_AlwaysStepZero() {
            var curve = Curve();

            Assert.AreEqual(0, curve.StepFor(500f));
            Assert.AreEqual(float.PositiveInfinity, curve.ThresholdFor(1));
        }

        [Test]
        public void Define_ReplacesExistingTable() {
            var curve = Curve(100f);
            curve.Define(50f, 50f);

            Assert.AreEqual(2, curve.StepCount);
            Assert.AreEqual(2, curve.StepFor(100f));
        }
    }
}
