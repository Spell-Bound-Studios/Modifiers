// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class ContributionTests {
        [Test]
        public void Of_ConvertsValueToInternal() {
            var contribution = Contribution.Of(ContributionType.Flat, 2.5f);

            Assert.AreEqual(StatSettings.ToInternal(2.5f), contribution.ValueInternal);
            Assert.AreEqual(ContributionType.Flat, contribution.Type);
        }

        [Test]
        public void Of_Defaults_AreNoneAndUnconditional() {
            var contribution = Contribution.Of(ContributionType.Flat, 1f);

            Assert.AreEqual(Contribution.None, contribution.SourceId);
            Assert.IsFalse(contribution.IsConditional);
        }

        [Test]
        public void IsConditional_WithCondition_IsTrue() {
            var contribution = Contribution.Of(ContributionType.Flat, 1f, 5u, new StubCondition());

            Assert.IsTrue(contribution.IsConditional);
            Assert.AreEqual(5u, contribution.SourceId);
        }
    }
}