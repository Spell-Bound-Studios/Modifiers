// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class StatSettingsTests {
        [Test]
        public void Precision_DefaultsToTenThousand() {
            Assert.AreEqual(10000, StatSettings.Precision);
        }

        [Test]
        public void ToInternal_ToExternal_RoundTrips() {
            Assert.AreEqual(12.34f, StatSettings.ToExternal(StatSettings.ToInternal(12.34f)), 0.0001f);
        }

        [Test]
        public void ToInternal_Negative_IsSymmetric() {
            Assert.AreEqual(-StatSettings.ToInternal(12.34f), StatSettings.ToInternal(-12.34f));
        }

        [Test]
        public void SetDecimalPrecision_SameValue_AllowedAfterConversions() {
            StatSettings.ToInternal(1f);

            Assert.DoesNotThrow(() => StatSettings.SetDecimalPrecision(4));
        }

        [Test]
        public void SetDecimalPrecision_ChangeAfterConversions_Throws() {
            StatSettings.ToInternal(1f);

            Assert.Throws<System.InvalidOperationException>(() => StatSettings.SetDecimalPrecision(2));
        }
    }
}
