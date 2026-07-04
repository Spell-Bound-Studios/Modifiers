// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class StageTests {
        [Test]
        public void Process_RunsChildrenInAscendingPriorityOrder() {
            var log = new List<string>();
            var stage = new Stage(1u);
            stage.Add(new RecordingLeaf(log, "late"), 5);
            stage.Add(new RecordingLeaf(log, "first"), -1);
            stage.Add(new RecordingLeaf(log, "mid"));

            stage.Process(new CircuitContext());

            CollectionAssert.AreEqual(new[] { "first", "mid", "late" }, log);
        }

        [Test]
        public void Process_TiesRunInGrantOrder() {
            var log = new List<string>();
            var stage = new Stage(1u);
            stage.Add(new RecordingLeaf(log, "a"), 3);
            stage.Add(new RecordingLeaf(log, "b"), 3);
            stage.Add(new RecordingLeaf(log, "c"), 3);

            stage.Process(new CircuitContext());

            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, log);
        }

        [Test]
        public void RemoveBySource_RevokesGrants() {
            var stage = new Stage(1u);
            var kept = new RecordingLeaf();
            var revoked = new RecordingLeaf();
            stage.Add(kept, 0, 10u);
            stage.Add(revoked, 0, 20u);

            Assert.AreEqual(1, stage.RemoveBySource(20u));

            stage.Process(new CircuitContext());

            Assert.AreEqual(1, kept.ProcessCount);
            Assert.AreEqual(0, revoked.ProcessCount);
        }

        [Test]
        public void RemoveBySource_InnateId_RemovesNothing() {
            var stage = new Stage(1u);
            var innate = new RecordingLeaf();
            stage.Add(innate);

            Assert.AreEqual(0, stage.RemoveBySource(Contribution.Innate));

            stage.Process(new CircuitContext());

            Assert.AreEqual(1, innate.ProcessCount);
        }

        [Test]
        public void Children_ExposesLiveViewInPriorityOrder() {
            var stage = new Stage(1u);
            var late = new RecordingLeaf();
            var early = new RecordingLeaf();
            stage.Add(late, 5, 10u);
            stage.Add(early, -5, 20u);

            Assert.AreEqual(2, stage.Children.Count);
            Assert.AreSame(early, stage.Children[0]);
            Assert.AreSame(late, stage.Children[1]);

            stage.RemoveBySource(10u);

            Assert.AreEqual(1, stage.Children.Count);
            Assert.AreSame(early, stage.Children[0]);
        }

        [Test]
        public void Add_AfterProcess_IncludesNewChild() {
            var stage = new Stage(1u);
            var first = new RecordingLeaf();
            stage.Add(first);
            stage.Process(new CircuitContext());

            var second = new RecordingLeaf();
            stage.Add(second);
            stage.Process(new CircuitContext());

            Assert.AreEqual(2, first.ProcessCount);
            Assert.AreEqual(1, second.ProcessCount);
        }
    }
}
