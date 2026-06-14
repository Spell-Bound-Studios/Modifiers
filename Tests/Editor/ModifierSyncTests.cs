// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Editor.Tests {
    /// <summary>
    /// Pins the owner→satellite sync contract end-to-end over real containers: generation-gated
    /// reconcile, full-resync idempotence, id-tracked removal, independence across receivers, and
    /// re-parenting between caches.
    /// </summary>
    public class ModifierSyncTests {
        private const uint StatA = 1u;
        private const uint StatB = 2u;

        private static (TestTarget target, StatOwnerBehaviourA owner) MakeTarget() {
            var target = new TestTarget();
            var owner = new StatOwnerBehaviourA((StatA, 100f));
            target.Behaviours.Add(owner);

            return (target, owner);
        }

        [Test]
        public void FirstReconcile_AppliesCacheContents() {
            var (target, owner) = MakeTarget();
            var receiver = new ModifierReceiver(target);
            var cache = new ModifierCache();
            cache.Add(new TestStatModifier(StatA, ModifierType.Flat, 10f));

            Assert.IsTrue(receiver.Reconcile(cache));
            Assert.AreEqual(110f, owner.GetValue(StatA));
            Assert.AreEqual(1, receiver.InjectedCount);
        }

        [Test]
        public void SameGeneration_ReconcileNoOps() {
            var (target, owner) = MakeTarget();
            var receiver = new ModifierReceiver(target);
            var cache = new ModifierCache();
            cache.Add(new TestStatModifier(StatA, ModifierType.Flat, 10f));

            receiver.Reconcile(cache);

            Assert.IsFalse(receiver.Reconcile(cache));
            Assert.AreEqual(110f, owner.GetValue(StatA));
        }

        [Test]
        public void InvalidateThenReconcile_DoesNotDoubleStack() {
            var (target, owner) = MakeTarget();
            var receiver = new ModifierReceiver(target);
            var cache = new ModifierCache();
            cache.Add(new TestStatModifier(StatA, ModifierType.Flat, 10f));

            receiver.Reconcile(cache);
            receiver.Invalidate();

            Assert.IsTrue(receiver.Reconcile(cache));
            Assert.AreEqual(110f, owner.GetValue(StatA));
        }

        [Test]
        public void CacheMutation_BumpsGeneration_NextReconcilePicksUp() {
            var (target, owner) = MakeTarget();
            var receiver = new ModifierReceiver(target);
            var cache = new ModifierCache();
            cache.Add(new TestStatModifier(StatA, ModifierType.Flat, 10f));

            receiver.Reconcile(cache);
            cache.Add(new TestStatModifier(StatA, ModifierType.Increased, 0.5f));

            Assert.IsTrue(receiver.Reconcile(cache));
            Assert.AreEqual(165f, owner.GetValue(StatA));
        }

        [Test]
        public void CacheRemoval_PropagatesWithoutTheInstance() {
            var (target, owner) = MakeTarget();
            var receiver = new ModifierReceiver(target);
            var cache = new ModifierCache();
            var mod = new TestStatModifier(StatA, ModifierType.Flat, 10f);
            cache.Add(mod);

            receiver.Reconcile(cache);
            cache.Remove(mod);

            Assert.IsTrue(receiver.Reconcile(cache));
            Assert.AreEqual(100f, owner.GetValue(StatA));
            Assert.AreEqual(0, receiver.InjectedCount);
        }

        [Test]
        public void TwoReceivers_OneCache_AreIndependent() {
            var (targetA, ownerA) = MakeTarget();
            var (targetB, ownerB) = MakeTarget();
            var receiverA = new ModifierReceiver(targetA);
            var receiverB = new ModifierReceiver(targetB);
            var cache = new ModifierCache();
            cache.Add(new TestStatModifier(StatA, ModifierType.Flat, 10f));

            receiverA.Reconcile(cache);
            receiverB.Reconcile(cache);

            Assert.AreEqual(110f, ownerA.GetValue(StatA));
            Assert.AreEqual(110f, ownerB.GetValue(StatA));

            receiverB.Detach();

            Assert.AreEqual(110f, ownerA.GetValue(StatA));
            Assert.AreEqual(100f, ownerB.GetValue(StatA));
        }

        [Test]
        public void Detach_SweepsAndForgets_ReconcileStartsFresh() {
            var (target, owner) = MakeTarget();
            var receiver = new ModifierReceiver(target);
            var cache = new ModifierCache();
            cache.Add(new TestStatModifier(StatA, ModifierType.Flat, 10f));

            receiver.Reconcile(cache);
            receiver.Detach();

            Assert.AreEqual(0, receiver.InjectedCount);
            Assert.AreEqual(100f, owner.GetValue(StatA));

            Assert.IsTrue(receiver.Reconcile(cache));
            Assert.AreEqual(110f, owner.GetValue(StatA));
        }

        [Test]
        public void BehaviourSetChange_PickedUpViaInvalidate() {
            var (target, _) = MakeTarget();
            var receiver = new ModifierReceiver(target);
            var cache = new ModifierCache();
            cache.Add(new TestStatModifier(StatB, ModifierType.Flat, 10f));

            receiver.Reconcile(cache);

            Assert.AreEqual(1, receiver.InjectedCount);

            var late = new StatOwnerBehaviourB((StatB, 50f));
            target.Behaviours.Add(late);
            receiver.Invalidate();

            Assert.IsTrue(receiver.Reconcile(cache));
            Assert.AreEqual(60f, late.GetValue(StatB));
        }

        [Test]
        public void Reparenting_SweepsOldOwner_AppliesNew() {
            var (target, owner) = MakeTarget();
            var receiver = new ModifierReceiver(target);
            var cacheA = new ModifierCache();
            cacheA.Add(new TestStatModifier(StatA, ModifierType.Flat, 10f));
            var cacheB = new ModifierCache();
            cacheB.Add(new TestStatModifier(StatA, ModifierType.Flat, 20f));

            receiver.Reconcile(cacheA);

            Assert.AreEqual(110f, owner.GetValue(StatA));

            Assert.IsTrue(receiver.Reconcile(cacheB));
            Assert.AreEqual(120f, owner.GetValue(StatA));
        }
    }
}
