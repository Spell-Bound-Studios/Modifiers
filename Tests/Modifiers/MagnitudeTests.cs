// Copyright 2026 Spellbound Studio Inc.

using System.Reflection;
using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class MagnitudeTests {
        private static StatDefinition ArmorDef => StatRegistry.GetDefinition("sample_armor");
        private static StatDefinition HealthDef => StatRegistry.GetDefinition("sample_health");
        private static StatId Armor => new(ArmorDef.Hash);
        private static StatId Health => new(HealthDef.Hash);

        [Test]
        public void Fixed_AppliesConstant() {
            var def = Definitions.Create(
                Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Fixed(43f)));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 0f);

            def.Roll(new System.Random(7), 7u).ApplyTo(target, def);

            Assert.AreEqual(43f, target.GetValue(Armor));
        }

        [Test]
        public void Rolled_BakesInRange() {
            var def = Definitions.Create(
                Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Rolled(8f, 15f, 1f)));

            var rolled = def.Roll(new System.Random(7), 7u);

            Assert.AreEqual(1, rolled.baked.Length);
            Assert.GreaterOrEqual(rolled.baked[0].value, 8f);
            Assert.LessOrEqual(rolled.baked[0].value, 15f);
        }

        [Test]
        public void Derived_FixedCoefficient_ScalesLiveWithSource() {
            var def = Definitions.Create(Definitions.Single(
                ArmorDef, ContributionType.Flat, Definitions.Derived(Definitions.Fixed(1f), 10, HealthDef)));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 0f);
            target.Stats.SetBase(Health, 100f);

            def.Roll(new System.Random(7), 7u).ApplyTo(target, def);

            Assert.AreEqual(10f, target.GetValue(Armor));

            target.Stats.AddContribution(Health, ContributionType.Flat, 100f, 9u);

            Assert.AreEqual(20f, target.GetValue(Armor));
        }

        [Test]
        public void Derived_DefinitionPatch_AffectsExistingRoll() {
            var magnitude = Definitions.Derived(Definitions.Fixed(1f), 10, HealthDef);
            var def = Definitions.Create(Definitions.Single(ArmorDef, ContributionType.Flat, magnitude));
            var rolled = def.Roll(new System.Random(7), 7u);

            var before = new Modifiable();
            before.Stats.SetBase(Armor, 0f);
            before.Stats.SetBase(Health, 100f);
            rolled.ApplyTo(before, def);

            Assert.AreEqual(10f, before.GetValue(Armor));

            typeof(DerivedMagnitude)
                    .GetField("perPoints", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(magnitude, 20);

            var after = new Modifiable();
            after.Stats.SetBase(Armor, 0f);
            after.Stats.SetBase(Health, 100f);
            rolled.ApplyTo(after, def);

            Assert.AreEqual(5f, after.GetValue(Armor));
        }

        [Test]
        public void Derived_RolledCoefficient_Composes() {
            var def = Definitions.Create(Definitions.Single(
                ArmorDef, ContributionType.Flat, Definitions.Derived(Definitions.Rolled(1f, 2f, 1f), 10, HealthDef)));

            var rolled = def.Roll(new System.Random(7), 7u);

            Assert.AreEqual(1, rolled.baked.Length);
            var amount = rolled.baked[0].value;
            Assert.GreaterOrEqual(amount, 1f);
            Assert.LessOrEqual(amount, 2f);

            var target = new Modifiable();
            target.Stats.SetBase(Armor, 0f);
            target.Stats.SetBase(Health, 100f);
            rolled.ApplyTo(target, def);

            Assert.AreEqual(amount * 100f / 10f, target.GetValue(Armor), 0.001f);
        }

        [Test]
        public void Derived_Stepped_FloorsByBreakpoint() {
            var def = Definitions.Create(Definitions.Single(
                ArmorDef, ContributionType.Flat, Definitions.Derived(Definitions.Fixed(1f), 20, HealthDef, true)));
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 0f);
            target.Stats.SetBase(Health, 55f);

            def.Roll(new System.Random(7), 7u).ApplyTo(target, def);

            Assert.AreEqual(2f, target.GetValue(Armor));
        }

        [Test]
        public void Band_KeepOrdered_LowNeverExceedsHigh() {
            var def = Definitions.Create(Definitions.Band(
                ArmorDef, Definitions.Rolled(1f, 10f, 1f),
                HealthDef, Definitions.Rolled(1f, 10f, 1f)));

            for (var seed = 0; seed < 50; seed++) {
                var rolled = def.Roll(new System.Random(seed), 7u);

                Assert.AreEqual(2, rolled.baked.Length);
                Assert.LessOrEqual(rolled.baked[0].value, rolled.baked[1].value);
            }
        }

        [Test]
        public void Baked_KeyedToStat_SurvivesDefinitionReorder() {
            var armorSpec = Definitions.Single(ArmorDef, ContributionType.Flat, Definitions.Rolled(5f, 5f));
            var healthSpec = Definitions.Single(HealthDef, ContributionType.Flat, Definitions.Rolled(30f, 30f));
            var rolled = Definitions.Create(armorSpec, healthSpec).Roll(new System.Random(7), 7u);

            var reordered = Definitions.Create(healthSpec, armorSpec);
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 0f);
            target.Stats.SetBase(Health, 0f);
            rolled.ApplyTo(target, reordered);

            Assert.AreEqual(5f, target.GetValue(Armor));
            Assert.AreEqual(30f, target.GetValue(Health));
        }
    }
}
