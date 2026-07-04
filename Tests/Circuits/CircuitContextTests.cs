// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Spellbound.Modifiers.Tests {
    public class CircuitContextTests {
        [Test]
        public void Consequence_IsNullUntilNoted() {
            Assert.IsNull(new CircuitContext().Consequence);
        }

        [Test]
        public void Note_AppendsEntries() {
            var ctx = new CircuitContext();
            ctx.Note(5u, 1f);
            ctx.Note(7u, 12.5f);

            Assert.AreEqual(2, ctx.Consequence.Count);
            Assert.AreEqual(5u, ctx.Consequence[0].statHash);
            Assert.AreEqual(1f, ctx.Consequence[0].amount);
            Assert.AreEqual(7u, ctx.Consequence[1].statHash);
            Assert.AreEqual(12.5f, ctx.Consequence[1].amount);
        }

        [Test]
        public void Clear_DetachesPacketAndSubject_EmptiesConsequence() {
            var ctx = new CircuitContext {
                Packet = new List<StatAndValue>(),
                Subject = new Modifiable(),
                Owner = new Modifiable()
            };
            ctx.Note(5u, 1f);

            ctx.Clear();

            Assert.IsNull(ctx.Packet);
            Assert.IsNull(ctx.Subject);
            Assert.IsNull(ctx.Owner);
            Assert.AreEqual(0, ctx.Consequence.Count);
        }

        [Test]
        public void Note_AfterClear_ReusesTheSameList() {
            var ctx = new CircuitContext();
            ctx.Note(5u, 1f);
            var list = ctx.Consequence;

            ctx.Clear();
            ctx.Note(7u, 2f);

            Assert.AreSame(list, ctx.Consequence);
            Assert.AreEqual(1, ctx.Consequence.Count);
            Assert.AreEqual(7u, ctx.Consequence[0].statHash);
        }

        [Test]
        public void Note_OnReusedContext_DoesNotAllocate() {
            var ctx = new CircuitContext();
            ctx.Note(5u, 1f);
            ctx.Clear();

            Assert.That(() => {
                ctx.Note(5u, 1f);
                ctx.Clear();
            }, Is.Not.AllocatingGCMemory());
        }
    }
}
