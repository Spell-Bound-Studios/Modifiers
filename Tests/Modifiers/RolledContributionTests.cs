// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Spellbound.Core.Packing;

namespace Spellbound.Modifiers.Tests {
    public class RolledContributionTests {
        private static StatId Armor => new(StatRegistry.GetHash("sample_armor"));
        private static StatId Health => new(StatRegistry.GetHash("sample_health"));

        private static RolledContribution RoundTrip(in RolledContribution source) {
            var bytes = new byte[64];
            Span<byte> buffer = bytes;
            source.Pack(ref buffer);
            var written = bytes.Length - buffer.Length;

            Assert.AreEqual(source.PackedSize, written);

            var read = new ReadOnlySpan<byte>(bytes, 0, written);
            var copy = new RolledContribution();
            copy.Unpack(ref read);

            return copy;
        }

        [Test]
        public void PackUnpack_RoundTrips() {
            var source = new RolledContribution {
                statHash = 11u, type = ContributionType.Increased, sourceStatHash = 0u, value = 2.5f, sourceId = 7u
            };

            var copy = RoundTrip(source);

            Assert.AreEqual(11u, copy.statHash);
            Assert.AreEqual(ContributionType.Increased, copy.type);
            Assert.AreEqual(0u, copy.sourceStatHash);
            Assert.AreEqual(2.5f, copy.value);
            Assert.AreEqual(7u, copy.sourceId);
        }

        [Test]
        public void PackUnpack_RoundTrips_Derived() {
            var source = new RolledContribution {
                statHash = 11u, type = ContributionType.Flat, sourceStatHash = 99u, value = 0.1f, sourceId = 7u
            };

            var copy = RoundTrip(source);

            Assert.AreEqual(99u, copy.sourceStatHash);
            Assert.AreEqual(0.1f, copy.value);
        }

        [Test]
        public void TryApplyTo_SelfDescribing_NeedsNoSchema() {
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);

            var rolled = new RolledContribution {
                statHash = Armor.Hash, type = ContributionType.Flat, value = 15f, sourceId = 7u
            };

            Assert.IsTrue(rolled.TryApplyTo(target));
            Assert.AreEqual(115f, target.GetValue(Armor));
        }

        [Test]
        public void TryApplyTo_Derived_Scales() {
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 10f);
            target.Stats.SetBase(Health, 100f);

            var rolled = new RolledContribution {
                statHash = Armor.Hash, type = ContributionType.Flat, sourceStatHash = Health.Hash,
                value = 0.1f, sourceId = 7u
            };
            rolled.TryApplyTo(target);

            Assert.AreEqual(20f, target.GetValue(Armor));

            target.Stats.AddContribution(Health, ContributionType.Flat, 100f, 9u);

            Assert.AreEqual(30f, target.GetValue(Armor));
        }

        [Test]
        public void RemoveFrom_Strips() {
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            var rolled = new RolledContribution {
                statHash = Armor.Hash, type = ContributionType.Flat, value = 15f, sourceId = 7u
            };
            rolled.TryApplyTo(target);

            Assert.AreEqual(115f, target.GetValue(Armor));

            rolled.RemoveFrom(target);

            Assert.AreEqual(100f, target.GetValue(Armor));
        }

        [Test]
        public void ContributionRange_Roll_StaysInRangeAndSteps() {
            var range = new ContributionRange {
                stat = StatRegistry.GetDefinition("sample_armor"), type = ContributionType.Flat,
                min = 1f, max = 9f, step = 1f
            };
            var rng = new System.Random(1234);

            for (var i = 0; i < 50; i++) {
                var value = range.Roll(rng);

                Assert.AreEqual(UnityEngine.Mathf.Round(value), value);
                Assert.GreaterOrEqual(value, 1f);
                Assert.LessOrEqual(value, 9f);
            }
        }

        [Test]
        public void ContributionRange_RollContribution_ProducesAndApplies() {
            var range = new ContributionRange {
                stat = StatRegistry.GetDefinition("sample_armor"), type = ContributionType.Flat, min = 5f, max = 5f
            };

            var rolled = range.RollContribution(new System.Random(1), 7u);

            Assert.AreEqual(Armor.Hash, rolled.statHash);
            Assert.AreEqual(ContributionType.Flat, rolled.type);
            Assert.AreEqual(0u, rolled.sourceStatHash);
            Assert.AreEqual(5f, rolled.value);
            Assert.AreEqual(7u, rolled.sourceId);

            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            rolled.TryApplyTo(target);

            Assert.AreEqual(105f, target.GetValue(Armor));
        }

        [Test]
        public void ContributionRange_RollContribution_CarriesDerivedSource() {
            var range = new ContributionRange {
                stat = StatRegistry.GetDefinition("sample_armor"), type = ContributionType.Flat,
                sourceStat = StatRegistry.GetDefinition("sample_health"), min = 0.1f, max = 0.1f
            };

            var rolled = range.RollContribution(new System.Random(1), 7u);

            Assert.AreEqual(Health.Hash, rolled.sourceStatHash);
        }

        [Test]
        public void SmartPack_MixedList_RestoresConcreteTypes() {
            IRolledModifier named = ModifierRegistry.GetDefinition("sample_thick_hide").Roll(new System.Random(7), 11u);
            IRolledModifier inline = new RolledContribution {
                statHash = Armor.Hash, type = ContributionType.Flat, value = 5f, sourceId = 12u
            };

            var restoredNamed = named.SmartPack().SmartUnpack();
            var restoredInline = inline.SmartPack().SmartUnpack();

            Assert.IsInstanceOf<RolledModifier>(restoredNamed);
            Assert.IsInstanceOf<RolledContribution>(restoredInline);
            Assert.AreEqual(12u, ((IRolledModifier)restoredInline).SourceId);
        }

        [Test]
        public void UniformList_NamedAndInline_BothApplyThroughInterface() {
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);

            var named = ModifierRegistry.GetDefinition("sample_thick_hide").Roll(new System.Random(7), 11u);
            var inline = new RolledContribution {
                statHash = Armor.Hash, type = ContributionType.Flat, value = 5f, sourceId = 12u
            };

            var modifiers = new List<IRolledModifier> { named, inline };

            foreach (var modifier in modifiers)
                modifier.TryApplyTo(target);

            Assert.AreEqual(100f + named.values[0] + 5f, target.GetValue(Armor), 0.001f);

            modifiers[1].RemoveFrom(target);

            Assert.AreEqual(100f + named.values[0], target.GetValue(Armor), 0.001f);
        }
    }
}
