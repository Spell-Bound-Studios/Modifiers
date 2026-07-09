// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;
using UnityEngine;

namespace Spellbound.Modifiers.Tests {
    public class ContributionSetTests {
        private static StatId Armor => new(StatRegistry.GetHash("sample_armor"));
        private static StatId Health => new(StatRegistry.GetHash("sample_health"));

        private static StatDefinition ArmorDef => StatRegistry.GetDefinition("sample_armor");
        private static StatDefinition HealthDef => StatRegistry.GetDefinition("sample_health");

        [Test]
        public void Inline_FixedValue_Applies() {
            var set = Definitions.Set(
                Definitions.Specification(ArmorDef, ContributionType.Flat, Definitions.Fixed(13f)));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 0f);

            set.RollAndApply(target, new System.Random(1), 99u);

            Assert.AreEqual(13f, target.GetValue(Armor));
        }

        [Test]
        public void Inline_RolledRange_StaysInRangeAndOnStep() {
            var set = Definitions.Set(
                Definitions.Specification(HealthDef, ContributionType.Flat, Definitions.Rolled(30f, 60f, 5f)));

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
        public void Inline_PairedLinkOrdered_HighNeverBelowLow() {
            var set = Definitions.Set(Definitions.Specification(
                ArmorDef, ContributionType.Flat, Definitions.Rolled(1f, 10f, 1f),
                HealthDef, Definitions.Rolled(1f, 10f, 1f), true));

            for (var seed = 0; seed < 50; seed++) {
                var target = new Modifiable();
                target.Stats.SetBase(Armor, 0f);
                target.Stats.SetBase(Health, 0f);

                set.RollAndApply(target, new System.Random(seed), 99u);

                Assert.LessOrEqual(target.GetValue(Armor), target.GetValue(Health));
            }
        }

        [Test]
        public void Inline_RemoveSource_StripsImplicits() {
            var set = Definitions.Set(
                Definitions.Specification(ArmorDef, ContributionType.Flat, Definitions.Fixed(13f)),
                Definitions.Specification(HealthDef, ContributionType.Flat, Definitions.Fixed(20f)));
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
            var set = Definitions.Set(
                Definitions.Specification(ArmorDef, ContributionType.Flat, Definitions.Fixed(13f)));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);

            set.RollAndApply(target, new System.Random(1), Contribution.None);

            Assert.AreEqual(113f, target.GetValue(Armor));

            using (new LogMute())
                target.RemoveSource(Contribution.None);

            Assert.AreEqual(113f, target.GetValue(Armor));
        }

        [Test]
        public void Inline_InvalidSpec_WarnsAndSkips() {
            var set = Definitions.Set(
                Definitions.Specification(ArmorDef, ContributionType.Flat, null),
                Definitions.Specification(HealthDef, ContributionType.Flat, Definitions.Fixed(20f)));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            target.Stats.SetBase(Health, 100f);

            using (new LogMute())
                set.RollAndApply(target, new System.Random(1), 99u);

            Assert.AreEqual(100f, target.GetValue(Armor));
            Assert.AreEqual(120f, target.GetValue(Health));
        }

        [Test]
        public void Inline_MatchesNamed_ForSameSpecs() {
            var specifications = new[] {
                Definitions.Specification(ArmorDef, ContributionType.Flat, Definitions.Rolled(8f, 15f, 1f)),
                Definitions.Specification(HealthDef, ContributionType.Flat, Definitions.Rolled(30f, 60f, 5f))
            };
            var definition = Definitions.Create(specifications);
            var set = Definitions.Set(specifications);

            var named = new Modifiable();
            named.Stats.SetBase(Armor, 100f);
            named.Stats.SetBase(Health, 100f);
            definition.Roll(new System.Random(7), 5u).ApplyTo(named, definition);

            var inline = new Modifiable();
            inline.Stats.SetBase(Armor, 100f);
            inline.Stats.SetBase(Health, 100f);
            set.RollAndApply(inline, new System.Random(7), 5u);

            Assert.AreEqual(named.GetValue(Armor), inline.GetValue(Armor), 0.001f);
            Assert.AreEqual(named.GetValue(Health), inline.GetValue(Health), 0.001f);
        }
    }
}
