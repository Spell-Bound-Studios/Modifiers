// Copyright 2026 Spellbound Studio Inc.

using System;
using NUnit.Framework;
using UnityEngine;

namespace Spellbound.Modifiers.Tests {
    public class ModifierGrantSetTests {
        private static StatId Armor => new(StatRegistry.GetHash("sample_armor"));
        private static StatId Health => new(StatRegistry.GetHash("sample_health"));

        private static StatDefinition ArmorDef => StatRegistry.GetDefinition("sample_armor");
        private static StatDefinition HealthDef => StatRegistry.GetDefinition("sample_health");

        [Test]
        public void Inline_FixedValue_Applies() {
            var set = Definitions.Grants(
                Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Fixed(13f)));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 0f);

            set.RollAndApply(target, new System.Random(1), 99u);

            Assert.AreEqual(13f, target.GetValue(Armor));
        }

        [Test]
        public void Inline_RolledRange_StaysInRangeAndOnStep() {
            var set = Definitions.Grants(
                Definitions.Single(HealthDef, ContributionType.Flat, Definitions.Rolled(30f, 60f, 5f)));

            for (var seed = 0; seed < 50; seed++) {
                var target = new Modifiable();
                target.Stats.SetBase(Health, 0f);

                set.RollAndApply(target, new System.Random(seed), 99u);
                var value = target.GetValue(Health);

                Assert.GreaterOrEqual(value, 30f);
                Assert.LessOrEqual(value, 60f);
                Assert.AreEqual(30f + Mathf.Round((value - 30f) / 5f) * 5f, value, 0.01f);
            }
        }

        [Test]
        public void Band_BothRolled_HighNeverBelowLow() {
            var set = Definitions.Grants(Definitions.Band(
                ArmorDef, Definitions.Rolled(1f, 10f, 1f),
                HealthDef, Definitions.Rolled(1f, 10f, 1f)));

            for (var seed = 0; seed < 50; seed++) {
                var target = new Modifiable();
                target.Stats.SetBase(Armor, 0f);
                target.Stats.SetBase(Health, 0f);

                set.RollAndApply(target, new System.Random(seed), 99u);

                Assert.LessOrEqual(target.GetValue(Armor), target.GetValue(Health));
            }
        }

        [Test]
        public void Band_FixedLowRolledHigh_ClampsHighUpToLow() {
            var set = Definitions.Grants(Definitions.Band(
                ArmorDef, Definitions.Fixed(11f),
                HealthDef, Definitions.Rolled(8f, 15f, 1f)));

            for (var seed = 0; seed < 50; seed++) {
                var rolled = set.Roll(new System.Random(seed), 99u);

                Assert.AreEqual(1, rolled.baked.Length);
                Assert.AreEqual(Health.Hash, rolled.baked[0].statHash);
                Assert.GreaterOrEqual(rolled.baked[0].value, 11f);
                Assert.LessOrEqual(rolled.baked[0].value, 15f);
            }
        }

        [Test]
        public void Band_RolledLowFixedHigh_ClampsLowDownToHigh() {
            var set = Definitions.Grants(Definitions.Band(
                ArmorDef, Definitions.Rolled(1f, 10f, 1f),
                HealthDef, Definitions.Fixed(5f)));

            for (var seed = 0; seed < 50; seed++) {
                var rolled = set.Roll(new System.Random(seed), 99u);

                Assert.AreEqual(1, rolled.baked.Length);
                Assert.AreEqual(Armor.Hash, rolled.baked[0].statHash);
                Assert.LessOrEqual(rolled.baked[0].value, 5f);
            }
        }

        [Test]
        public void Band_KeepOrderedWithDerivedEnd_IsInvalidAndSkipped() {
            var derived = Definitions.Derived(Definitions.Fixed(1f), 10, HealthDef);
            var ordered = Definitions.Band(ArmorDef, derived, HealthDef, Definitions.Rolled(1f, 10f, 1f));

            Assert.IsFalse(ordered.IsValid);

            var unordered = Definitions.Band(ArmorDef, Definitions.Derived(Definitions.Fixed(1f), 10, HealthDef),
                HealthDef, Definitions.Rolled(1f, 10f, 1f), keepOrdered: false);

            Assert.IsTrue(unordered.IsValid);

            RolledGrants rolled;

            using (new LogMute())
                rolled = Definitions.Grants(ordered).Roll(new System.Random(1), 5u);

            Assert.IsTrue(rolled.IsEmpty);
        }

        [Test]
        public void Band_FixedPairInverted_KeepOrderedIsInvalid() {
            var inverted = Definitions.Band(ArmorDef, Definitions.Fixed(19f), HealthDef, Definitions.Fixed(11f));

            Assert.IsFalse(inverted.IsValid);
            Assert.IsTrue(Definitions.Band(ArmorDef, Definitions.Fixed(11f), HealthDef, Definitions.Fixed(19f)).IsValid);
        }

        [Test]
        public void Multi_SharedRoll_SameValueAcrossStats() {
            var set = Definitions.Grants(
                Definitions.Multi(ContributionType.Flat, Definitions.Rolled(10f, 20f, 1f), ArmorDef, HealthDef));

            for (var seed = 0; seed < 20; seed++) {
                var rolled = set.Roll(new System.Random(seed), 99u);

                Assert.AreEqual(2, rolled.baked.Length);
                Assert.AreEqual(rolled.baked[0].value, rolled.baked[1].value);
                Assert.AreNotEqual(rolled.baked[0].statHash, rolled.baked[1].statHash);

                var target = new Modifiable();
                target.Stats.SetBase(Armor, 0f);
                target.Stats.SetBase(Health, 0f);
                set.Apply(target, 99u, rolled);

                Assert.AreEqual(target.GetValue(Armor), target.GetValue(Health));
            }
        }

        [Test]
        public void Multi_FixedAmount_AppliesToEveryStat() {
            var set = Definitions.Grants(
                Definitions.Multi(ContributionType.Flat, Definitions.Fixed(10f), ArmorDef, HealthDef));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            target.Stats.SetBase(Health, 100f);

            Assert.IsTrue(set.Roll(new System.Random(1), 99u).IsEmpty);

            set.RollAndApply(target, new System.Random(1), 99u);

            Assert.AreEqual(110f, target.GetValue(Armor));
            Assert.AreEqual(110f, target.GetValue(Health));
        }

        [Test]
        public void Inline_RemoveSource_StripsImplicits() {
            var set = Definitions.Grants(
                Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Fixed(13f)),
                Definitions.Single(HealthDef, ContributionType.Flat, Definitions.Fixed(20f)));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            target.Stats.SetBase(Health, 100f);

            set.RollAndApply(target, new System.Random(1), 99u);

            Assert.AreEqual(113f, target.GetValue(Armor));
            Assert.AreEqual(120f, target.GetValue(Health));

            target.RemoveSource(99u);

            Assert.AreEqual(100f, target.GetValue(Armor));
            Assert.AreEqual(100f, target.GetValue(Health));
        }

        [Test]
        public void Inline_UnderNone_AppliedButNotRemovable() {
            var set = Definitions.Grants(
                Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Fixed(13f)));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);

            set.RollAndApply(target, new System.Random(1), Contribution.None);

            Assert.AreEqual(113f, target.GetValue(Armor));

            using (new LogMute())
                target.RemoveSource(Contribution.None);

            Assert.AreEqual(113f, target.GetValue(Armor));
        }

        [Test]
        public void InvalidGrant_WarnsAndSkips() {
            var set = Definitions.Grants(
                Definitions.Single(ArmorDef, ContributionType.Flat, null),
                Definitions.Named(null),
                Definitions.Single(HealthDef, ContributionType.Flat, Definitions.Fixed(20f)));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            target.Stats.SetBase(Health, 100f);

            using (new LogMute())
                set.RollAndApply(target, new System.Random(1), 99u);

            Assert.AreEqual(100f, target.GetValue(Armor));
            Assert.AreEqual(120f, target.GetValue(Health));
        }

        [Test]
        public void Inline_MatchesNamed_ForSameSpecifications() {
            ContributionSpecification[] Specifications() => new ContributionSpecification[] {
                Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Rolled(8f, 15f, 1f)),
                Definitions.Single(HealthDef, ContributionType.Flat, Definitions.Rolled(30f, 60f, 5f))
            };

            var named = new Modifiable();
            named.Stats.SetBase(Armor, 100f);
            named.Stats.SetBase(Health, 100f);
            Definitions.Grants(Definitions.Named(Definitions.Create(Specifications())))
                    .RollAndApply(named, new System.Random(7), 5u);

            var specifications = Specifications();
            var inline = new Modifiable();
            inline.Stats.SetBase(Armor, 100f);
            inline.Stats.SetBase(Health, 100f);
            Definitions.Grants(specifications[0], specifications[1])
                    .RollAndApply(inline, new System.Random(7), 5u);

            Assert.AreEqual(named.GetValue(Armor), inline.GetValue(Armor), 0.001f);
            Assert.AreEqual(named.GetValue(Health), inline.GetValue(Health), 0.001f);
        }

        [Test]
        public void Roll_SameSeed_IsDeterministic() {
            var definition = Definitions.Create(
                Definitions.Single(HealthDef, ContributionType.Flat, Definitions.Rolled(30f, 60f, 5f)));
            var set = Definitions.Grants(
                Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Rolled(1f, 10f, 1f)),
                Definitions.Named(definition));

            var first = set.Roll(new System.Random(11), 5u);
            var second = set.Roll(new System.Random(11), 5u);

            Assert.AreEqual(first.baked.Length, second.baked.Length);

            for (var i = 0; i < first.baked.Length; i++) {
                Assert.AreEqual(first.baked[i].statHash, second.baked[i].statHash);
                Assert.AreEqual(first.baked[i].value, second.baked[i].value);
            }

            Assert.AreEqual(first.modifiers.Length, second.modifiers.Length);
            Assert.AreEqual(first.modifiers[0].baked[0].value, second.modifiers[0].baked[0].value);
        }

        [Test]
        public void Roll_InlineFixedOnly_HasNothingToPersist() {
            var set = Definitions.Grants(
                Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Fixed(13f)),
                Definitions.Single(HealthDef, ContributionType.Flat, Definitions.Fixed(20f)));

            Assert.IsTrue(set.Roll(new System.Random(1), 5u).IsEmpty);
        }

        [Test]
        public void Roll_ThenApply_MatchesRollAndApply() {
            var definition = Definitions.Create(
                Definitions.Single(HealthDef, ContributionType.Flat, Definitions.Rolled(30f, 60f, 5f)));

            ModifierGrantSet Set() => Definitions.Grants(
                Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Rolled(1f, 10f, 1f)),
                Definitions.Named(definition));

            var live = new Modifiable();
            live.Stats.SetBase(Armor, 100f);
            live.Stats.SetBase(Health, 100f);
            Set().RollAndApply(live, new System.Random(7), 5u);

            var set = Set();
            var hydrated = new Modifiable();
            hydrated.Stats.SetBase(Armor, 100f);
            hydrated.Stats.SetBase(Health, 100f);
            set.Apply(hydrated, 5u, set.Roll(new System.Random(7), 5u));

            Assert.AreEqual(live.GetValue(Armor), hydrated.GetValue(Armor), 0.001f);
            Assert.AreEqual(live.GetValue(Health), hydrated.GetValue(Health), 0.001f);
        }

        [Test]
        public void Apply_RemoveSource_StripsBothKinds() {
            var definition = Definitions.Create(
                Definitions.Single(HealthDef, ContributionType.Flat, Definitions.Rolled(30f, 60f, 5f)));
            var set = Definitions.Grants(
                Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Rolled(10f, 15f, 1f)),
                Definitions.Named(definition));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            target.Stats.SetBase(Health, 100f);

            set.Apply(target, 42u, set.Roll(new System.Random(3), 42u));

            Assert.Greater(target.GetValue(Armor), 100f);
            Assert.Greater(target.GetValue(Health), 100f);

            target.RemoveSource(42u);

            Assert.AreEqual(100f, target.GetValue(Armor));
            Assert.AreEqual(100f, target.GetValue(Health));
        }

        [Test]
        public void Apply_Reapply_RestoresSameValues() {
            var definition = Definitions.Create(
                Definitions.Single(HealthDef, ContributionType.Flat, Definitions.Rolled(30f, 60f, 5f)));
            var set = Definitions.Grants(
                Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Rolled(10f, 15f, 1f)),
                Definitions.Named(definition));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 0f);
            target.Stats.SetBase(Health, 0f);
            var rolled = set.Roll(new System.Random(9), 7u);

            set.Apply(target, 7u, rolled);
            var firstArmor = target.GetValue(Armor);
            var firstHealth = target.GetValue(Health);
            target.RemoveSource(7u);
            set.Apply(target, 7u, rolled);

            Assert.AreEqual(firstArmor, target.GetValue(Armor));
            Assert.AreEqual(firstHealth, target.GetValue(Health));
        }

        [Test]
        public void Roll_DuplicateRolledStat_KeepsFirstRoll() {
            var set = Definitions.Grants(
                Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Rolled(1f, 10f, 1f)),
                Definitions.Single(ArmorDef, ContributionType.More, Definitions.Rolled(1f, 10f, 1f)));

            RolledGrants rolled;

            using (new LogMute())
                rolled = set.Roll(new System.Random(1), 5u);

            Assert.AreEqual(1, rolled.baked.Length);
            Assert.AreEqual(Armor.Hash, rolled.baked[0].statHash);
        }

        [Test]
        public void Roll_DuplicateNamedDefinition_KeepsFirstRecord() {
            var definition = Definitions.Create(
                Definitions.Single(HealthDef, ContributionType.Flat, Definitions.Rolled(30f, 60f, 5f)));
            var set = Definitions.Grants(Definitions.Named(definition), Definitions.Named(definition));

            RolledGrants rolled;

            using (new LogMute())
                rolled = set.Roll(new System.Random(1), 5u);

            Assert.AreEqual(1, rolled.modifiers.Length);
        }

        [Test]
        public void Apply_MissingNamedRecord_WarnsAndSkipsNamed() {
            var definition = Definitions.Create(
                Definitions.Single(HealthDef, ContributionType.Flat, Definitions.Rolled(30f, 60f, 5f)));
            var set = Definitions.Grants(
                Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Fixed(13f)),
                Definitions.Named(definition));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            target.Stats.SetBase(Health, 100f);
            var partial = new RolledGrants
                    { baked = set.Roll(new System.Random(1), 5u).baked, modifiers = Array.Empty<RolledModifier>() };

            using (new LogMute())
                set.Apply(target, 5u, partial);

            Assert.AreEqual(113f, target.GetValue(Armor));
            Assert.AreEqual(100f, target.GetValue(Health));
        }

        [Test]
        public void Apply_UnderNewSourceId_RekeysNamedRecords() {
            var definition = Definitions.Create(
                Definitions.Single(HealthDef, ContributionType.Flat, Definitions.Fixed(20f)));
            var set = Definitions.Grants(
                Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Fixed(13f)),
                Definitions.Named(definition));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            target.Stats.SetBase(Health, 100f);
            var rolled = set.Roll(new System.Random(1), Contribution.None);

            set.Apply(target, 42u, rolled);

            Assert.AreEqual(113f, target.GetValue(Armor));
            Assert.AreEqual(120f, target.GetValue(Health));
            Assert.AreEqual(Contribution.None, rolled.modifiers[0].sourceId);

            target.RemoveSource(42u);

            Assert.AreEqual(100f, target.GetValue(Armor));
            Assert.AreEqual(100f, target.GetValue(Health));
        }

        [Test]
        public void RolledGrants_PackUnpack_RoundTrips() {
            var rolled = new RolledGrants {
                baked = new[] {
                    new BakedRoll { statHash = 1u, value = 1.5f },
                    new BakedRoll { statHash = 2u, value = -2f }
                },
                modifiers = new[] {
                    new RolledModifier {
                        modifierHash = 123u,
                        sourceId = 77u,
                        baked = new[] { new BakedRoll { statHash = 3u, value = 40f } }
                    }
                }
            };
            var bytes = new byte[128];
            Span<byte> buffer = bytes;
            rolled.Pack(ref buffer);
            var written = bytes.Length - buffer.Length;

            Assert.AreEqual(rolled.PackedSize, written);

            var read = new ReadOnlySpan<byte>(bytes, 0, written);
            var copy = new RolledGrants();
            copy.Unpack(ref read);

            Assert.AreEqual(2, copy.baked.Length);
            Assert.AreEqual(2u, copy.baked[1].statHash);
            Assert.AreEqual(-2f, copy.baked[1].value);
            Assert.AreEqual(1, copy.modifiers.Length);
            Assert.AreEqual(123u, copy.modifiers[0].modifierHash);
            Assert.AreEqual(77u, copy.modifiers[0].sourceId);
            Assert.AreEqual(40f, copy.modifiers[0].baked[0].value);
        }
    }
}
