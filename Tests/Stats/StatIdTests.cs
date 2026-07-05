// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class StatIdTests {
        [Test]
        public void Equals_SameHash_IsTrue() {
            Assert.IsTrue(new StatId(5u).Equals(new StatId(5u)));
            Assert.IsTrue(new StatId(5u) == new StatId(5u));
        }

        [Test]
        public void Equals_DifferentHash_IsFalse() {
            Assert.IsFalse(new StatId(5u).Equals(new StatId(6u)));
            Assert.IsTrue(new StatId(5u) != new StatId(6u));
        }

        [Test]
        public void ImplicitConversion_YieldsHash() {
            uint hash = new StatId(5u);

            Assert.AreEqual(5u, hash);
        }

        [Test]
        public void WorksAsDictionaryKey() {
            var map = new Dictionary<StatId, int> { [new StatId(5u)] = 1 };

            Assert.AreEqual(1, map[new StatId(5u)]);
        }
    }
}