// Copyright 2026 Spellbound Studio Inc.

using System;
using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class RolledModifierTests {
        [Test]
        public void PackUnpack_RoundTrips() {
            var rolled = new RolledModifier {
                modifierHash = 123u,
                sourceId = 77u,
                baked = new[] {
                    new BakedRoll { statHash = 1u, value = 1.5f },
                    new BakedRoll { statHash = 2u, value = -2f },
                    new BakedRoll { statHash = 3u, value = 0f }
                }
            };
            var bytes = new byte[64];
            Span<byte> buffer = bytes;
            rolled.Pack(ref buffer);
            var written = bytes.Length - buffer.Length;

            var read = new ReadOnlySpan<byte>(bytes, 0, written);
            var copy = new RolledModifier();
            copy.Unpack(ref read);

            Assert.AreEqual(123u, copy.modifierHash);
            Assert.AreEqual(77u, copy.sourceId);
            Assert.AreEqual(3, copy.baked.Length);
            Assert.AreEqual(2u, copy.baked[1].statHash);
            Assert.AreEqual(-2f, copy.baked[1].value);
        }

        [Test]
        public void Pack_NullBaked_RoundTripsEmpty() {
            var rolled = new RolledModifier { modifierHash = 5u, sourceId = 6u, baked = null };
            var bytes = new byte[32];
            Span<byte> buffer = bytes;
            rolled.Pack(ref buffer);
            var written = bytes.Length - buffer.Length;

            var read = new ReadOnlySpan<byte>(bytes, 0, written);
            var copy = new RolledModifier();
            copy.Unpack(ref read);

            Assert.AreEqual(0, copy.baked.Length);
        }

        [Test]
        public void TryApplyTo_UnknownHash_ReturnsFalse() {
            using (new LogMute()) {
                var rolled = new RolledModifier
                        { modifierHash = 999999u, sourceId = 1u, baked = Array.Empty<BakedRoll>() };

                Assert.IsFalse(rolled.TryApplyTo(new Modifiable()));
            }
        }
    }
}
