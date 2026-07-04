// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class ModifiableTests {
        private static readonly StatId Armor = new(1u);

        [Test]
        public void GetValue_ReadsStats() {
            var modifiable = new Modifiable();
            modifiable.Stats.SetBase(Armor, 42f);

            Assert.AreEqual(42f, modifiable.GetValue(Armor));
        }

        [Test]
        public void Run_UnknownIdentity_DoesNotThrow() {
            Assert.DoesNotThrow(() => new Modifiable().Run(5u, new CircuitContext()));
        }

        [Test]
        public void Run_SetsSubjectOnContext() {
            var modifiable = new Modifiable();
            var leaf = new RecordingLeaf();
            modifiable.CircuitFor(5u).Root = leaf;

            modifiable.Run(5u, new CircuitContext());

            Assert.AreSame(modifiable, leaf.LastSubject);
        }

        [Test]
        public void Run_RestoresSubjectAfterNestedRun() {
            var outer = new Modifiable();
            var inner = new Modifiable();
            var innerLeaf = new RecordingLeaf();
            inner.CircuitFor(1u).Root = innerLeaf;

            var afterNested = new RecordingLeaf();
            outer.CircuitFor(1u).Root = new Sequence(new RunOtherLeaf(inner, 1u), afterNested);

            var ctx = new CircuitContext();
            outer.Run(1u, ctx);

            Assert.AreSame(inner, innerLeaf.LastSubject);
            Assert.AreSame(outer, afterNested.LastSubject);
            Assert.IsNull(ctx.Subject);
        }

        [Test]
        public void RemoveSource_StripsStatsAndCircuitGrants() {
            var modifiable = new Modifiable();
            const uint item = 77u;
            modifiable.Stats.SetBase(Armor, 100f);
            modifiable.Stats.AddModifier(Armor, ModifierType.Flat, 50f, item);

            var circuit = modifiable.CircuitFor(5u);
            var stage = circuit.DefineStage(1u);
            circuit.Root = stage;
            var leaf = new RecordingLeaf();
            stage.Add(leaf, 0, item);

            var ctx = new CircuitContext();
            modifiable.Run(5u, ctx);

            Assert.AreEqual(150f, modifiable.GetValue(Armor));
            Assert.AreEqual(1, leaf.ProcessCount);

            Assert.AreEqual(2, modifiable.RemoveSource(item));

            modifiable.Run(5u, ctx);

            Assert.AreEqual(100f, modifiable.GetValue(Armor));
            Assert.AreEqual(1, leaf.ProcessCount);
        }
    }
}
