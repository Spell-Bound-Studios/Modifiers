// Copyright 2026 Spellbound Studio Inc.

using System;
using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class TimedModifierSetTests {
        private static StatId Armor => new(StatRegistry.GetHash("sample_armor"));
        private static StatId Health => new(StatRegistry.GetHash("sample_health"));

        private static RolledModifier Roll(string modifierName, uint sourceId, int seed = 7) =>
                ModifierRegistry.GetDefinition(modifierName).Roll(new Random(seed), sourceId);

        [Test]
        public void Apply_AddsContributions() {
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            var set = new TimedModifierSet(target);

            var rolled = Roll("sample_thick_hide", 11u);
            set.Apply(rolled, 5f);

            Assert.AreEqual(100f + rolled.values[0], target.GetValue(Armor), 0.001f);
            Assert.AreEqual(1, set.Active.Count);
        }

        [Test]
        public void Tick_ExpiresAndStrips() {
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            var set = new TimedModifierSet(target);
            set.Apply(Roll("sample_thick_hide", 11u), 5f);

            set.Tick(4.9f);

            Assert.AreEqual(1, set.Active.Count);

            set.Tick(0.2f);

            Assert.AreEqual(0, set.Active.Count);
            Assert.AreEqual(100f, target.GetValue(Armor), 0.001f);
        }

        [Test]
        public void Apply_SameModifier_RefreshesInsteadOfStacking() {
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            var set = new TimedModifierSet(target);

            set.Apply(Roll("sample_thick_hide", 11u, 7), 5f);
            set.Tick(4f);

            var second = Roll("sample_thick_hide", 12u, 8);
            set.Apply(second, 5f);

            Assert.AreEqual(1, set.Active.Count);
            Assert.AreEqual(100f + second.values[0], target.GetValue(Armor), 0.001f);

            set.Tick(4.9f);

            Assert.AreEqual(1, set.Active.Count);

            set.Tick(0.2f);

            Assert.AreEqual(0, set.Active.Count);
            Assert.AreEqual(100f, target.GetValue(Armor), 0.001f);
        }

        [Test]
        public void Dispel_RemovesByHash() {
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            var set = new TimedModifierSet(target);
            set.Apply(Roll("sample_thick_hide", 11u), 5f);

            var hash = ModifierRegistry.GetDefinition("sample_thick_hide").Hash;

            Assert.AreEqual(1, set.Dispel(hash));
            Assert.AreEqual(0, set.Active.Count);
            Assert.AreEqual(100f, target.GetValue(Armor), 0.001f);
        }

        [Test]
        public void Clear_StripsEverything() {
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            target.Stats.SetBase(Health, 100f);
            var set = new TimedModifierSet(target);
            set.Apply(Roll("sample_thick_hide", 11u), 5f);
            set.Apply(Roll("sample_vigorous", 12u), 5f);

            set.Clear();

            Assert.AreEqual(0, set.Active.Count);
            Assert.AreEqual(100f, target.GetValue(Armor), 0.001f);
            Assert.AreEqual(100f, target.GetValue(Health), 0.001f);
        }

        [Test]
        public void Changed_FiresOnApplyExpireAndDispel() {
            var target = new Modifiable();
            var set = new TimedModifierSet(target);
            var count = 0;
            set.Changed += () => count++;

            set.Apply(Roll("sample_thick_hide", 11u), 1f);
            set.Tick(1.1f);
            set.Apply(Roll("sample_thick_hide", 12u), 5f);
            set.Dispel(ModifierRegistry.GetDefinition("sample_thick_hide").Hash);

            Assert.AreEqual(4, count);
        }

        [Test]
        public void Restore_PreservesRemaining() {
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            var set = new TimedModifierSet(target);

            var entry = new TimedModifier { modifier = Roll("sample_thick_hide", 11u), duration = 5f, remaining = 2f };
            set.Restore(entry);

            Assert.Greater(target.GetValue(Armor), 100f);

            set.Tick(1.9f);

            Assert.AreEqual(1, set.Active.Count);

            set.Tick(0.2f);

            Assert.AreEqual(0, set.Active.Count);
            Assert.AreEqual(100f, target.GetValue(Armor), 0.001f);
        }

        [Test]
        public void TimedModifier_PackUnpack_RoundTrips() {
            var entry = new TimedModifier {
                modifier = new RolledModifier { modifierHash = 123u, sourceId = 77u, values = new[] { 12f } },
                duration = 5f,
                remaining = 2.5f
            };

            var bytes = new byte[64];
            Span<byte> buffer = bytes;
            entry.Pack(ref buffer);
            var written = bytes.Length - buffer.Length;

            var read = new ReadOnlySpan<byte>(bytes, 0, written);
            var copy = new TimedModifier();
            copy.Unpack(ref read);

            Assert.AreEqual(123u, copy.modifier.modifierHash);
            Assert.AreEqual(77u, copy.modifier.sourceId);
            CollectionAssert.AreEqual(entry.modifier.values, copy.modifier.values);
            Assert.AreEqual(5f, copy.duration);
            Assert.AreEqual(2.5f, copy.remaining);
        }
    }
}