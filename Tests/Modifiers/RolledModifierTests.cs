// Copyright 2026 Spellbound Studio Inc.

using System;
using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class RolledModifierTests {
        [Test]
        public void PackUnpack_RoundTrips() {
            var rolled = new RolledModifier { modifierHash = 123u, sourceId = 77u, values = new[] { 1.5f, -2f, 0f } };
            var bytes = new byte[64];
            Span<byte> buffer = bytes;
            rolled.Pack(ref buffer);
            var written = bytes.Length - buffer.Length;

            var read = new ReadOnlySpan<byte>(bytes, 0, written);
            var copy = new RolledModifier();
            copy.Unpack(ref read);

            Assert.AreEqual(123u, copy.modifierHash);
            Assert.AreEqual(77u, copy.sourceId);
            CollectionAssert.AreEqual(rolled.values, copy.values);
        }

        [Test]
        public void Pack_NullValues_RoundTripsEmpty() {
            var rolled = new RolledModifier { modifierHash = 5u, sourceId = 6u, values = null };
            var bytes = new byte[32];
            Span<byte> buffer = bytes;
            rolled.Pack(ref buffer);
            var written = bytes.Length - buffer.Length;

            var read = new ReadOnlySpan<byte>(bytes, 0, written);
            var copy = new RolledModifier();
            copy.Unpack(ref read);

            Assert.AreEqual(0, copy.values.Length);
        }

        [Test]
        public void TryApplyTo_UnknownHash_ReturnsFalse() {
            using (new LogMute()) {
                var rolled = new RolledModifier { modifierHash = 999999u, sourceId = 1u, values = Array.Empty<float>() };

                Assert.IsFalse(rolled.TryApplyTo(new Modifiable()));
            }
        }
    }
}
