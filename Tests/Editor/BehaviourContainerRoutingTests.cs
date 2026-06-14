// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Editor.Tests {
    /// <summary>
    /// Pins the container-level ownership dispatch: apply lands only on owners, removal is an
    /// identity-keyed sweep, and zero-owner calls touch nothing.
    /// </summary>
    public class BehaviourContainerRoutingTests {
        private const uint StatA = 1u;
        private const uint StatB = 2u;
        private const uint Shared = 3u;
        private const uint Unowned = 9u;

        private static (BehaviourContainer container, StatOwnerBehaviourA a, StatOwnerBehaviourB b) MakeContainer() {
            var a = new StatOwnerBehaviourA((StatA, 100f));
            var b = new StatOwnerBehaviourB((StatB, 50f));
            var container = new BehaviourContainer();
            container.Add(a);
            container.Add(b);

            return (container, a, b);
        }

        [Test]
        public void Apply_LandsOnOwnersOnly() {
            var (container, a, b) = MakeContainer();

            var owners = container.AddModifier(new StatModifierEntry(StatA, ModifierType.Flat, 10f));

            Assert.AreEqual(1, owners);
            Assert.AreEqual(110f, a.GetValue(StatA));
            Assert.AreEqual(50f, b.GetValue(StatB));
            Assert.IsFalse(b.HasBase(StatA));
        }

        [Test]
        public void Apply_DispatchesToEveryOwner() {
            var a = new StatOwnerBehaviourA((Shared, 100f));
            var b = new StatOwnerBehaviourB((Shared, 40f));
            var container = new BehaviourContainer();
            container.Add(a);
            container.Add(b);

            var owners = container.AddModifier(new StatModifierEntry(Shared, ModifierType.Flat, 10f));

            Assert.AreEqual(2, owners);
            Assert.AreEqual(110f, a.GetValue(Shared));
            Assert.AreEqual(50f, b.GetValue(Shared));
        }

        [Test]
        public void Apply_ZeroOwners_TouchesNothing() {
            var (container, a, b) = MakeContainer();

            var aFires = 0;
            var bFires = 0;
            a.OnStatsDirty += () => aFires++;
            b.OnStatsDirty += () => bFires++;

            var owners = container.AddModifier(new StatModifierEntry(Unowned, ModifierType.Flat, 10f));

            Assert.AreEqual(0, owners);
            Assert.AreEqual(0, aFires);
            Assert.AreEqual(0, bFires);
            Assert.AreEqual(100f, a.GetValue(StatA));
            Assert.AreEqual(50f, b.GetValue(StatB));
        }

        [Test]
        public void Remove_RestoresExactValues() {
            var (container, a, _) = MakeContainer();
            container.AddModifier(new StatModifierEntry(StatA, ModifierType.Flat, 10f, "x"));

            var removed = container.RemoveModifierByUniqueId("x");

            Assert.AreEqual(1, removed);
            Assert.AreEqual(100f, a.GetValue(StatA));
        }

        [Test]
        public void RemoveAbsentId_ReturnsZero_DirtiesNothing() {
            var (container, a, b) = MakeContainer();

            var aFires = 0;
            var bFires = 0;
            a.OnStatsDirty += () => aFires++;
            b.OnStatsDirty += () => bFires++;

            Assert.AreEqual(0, container.RemoveModifierByUniqueId("nope"));
            Assert.AreEqual(0, aFires);
            Assert.AreEqual(0, bFires);
        }

        [Test]
        public void Remove_SweepsSameIdAcrossOwners() {
            var a = new StatOwnerBehaviourA((Shared, 100f));
            var b = new StatOwnerBehaviourB((Shared, 40f));
            var container = new BehaviourContainer();
            container.Add(a);
            container.Add(b);

            Assert.AreEqual(2, container.AddModifier(new StatModifierEntry(Shared, ModifierType.Flat, 10f, "x")));
            Assert.AreEqual(2, container.RemoveModifierByUniqueId("x"));
            Assert.AreEqual(100f, a.GetValue(Shared));
            Assert.AreEqual(40f, b.GetValue(Shared));
        }
    }
}
