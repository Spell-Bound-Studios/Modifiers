// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class DerivedContributionTests {
        private static StatId Armor => new(StatRegistry.GetHash("sample_armor"));
        private static StatId Health => new(StatRegistry.GetHash("sample_health"));

        [Test]
        public void Derived_ScalesWithResolvedSource() {
            var m = new Modifiable();
            m.Stats.SetBase(Armor, 10f);
            m.Stats.SetBase(Health, 100f);

            m.Stats.AddDerived(Armor, ContributionType.Flat, Health, 0.1f, 1, false, Perspective.Owner);

            Assert.AreEqual(20f, m.GetValue(Armor));
        }

        [Test]
        public void Derived_IsLive_HelmetTest() {
            var m = new Modifiable();
            m.Stats.SetBase(Armor, 10f);
            m.Stats.SetBase(Health, 100f);
            m.Stats.AddDerived(Armor, ContributionType.Flat, Health, 0.1f, 1, false, Perspective.Owner);

            Assert.AreEqual(20f, m.GetValue(Armor));

            m.Stats.AddContribution(Health, ContributionType.Flat, 50f, 9u);

            Assert.AreEqual(25f, m.GetValue(Armor));
        }

        [Test]
        public void Derived_SourceFullyResolvesBeforeRatio() {
            var m = new Modifiable();
            m.Stats.SetBase(Armor, 10f);
            m.Stats.SetBase(Health, 100f);
            m.Stats.AddContribution(Health, ContributionType.Increased, 1f);
            m.Stats.AddDerived(Armor, ContributionType.Flat, Health, 0.1f, 1, false, Perspective.Owner);

            Assert.AreEqual(30f, m.GetValue(Armor));
        }

        [Test]
        public void Derived_FeedsIncreasedBucket() {
            var m = new Modifiable();
            m.Stats.SetBase(Armor, 100f);
            m.Stats.SetBase(Health, 200f);

            m.Stats.AddDerived(Armor, ContributionType.Increased, Health, 0.001f, 1, false, Perspective.Owner);

            Assert.AreEqual(120f, m.GetValue(Armor));
        }

        [Test]
        public void Derived_SourceReadsThroughChain() {
            var player = new Modifiable();
            player.Stats.SetBase(Health, 100f);

            var skill = new Modifiable { Parent = player };
            skill.Stats.SetBase(Armor, 10f);
            skill.Stats.AddDerived(Armor, ContributionType.Flat, Health, 0.1f, 1, false, Perspective.Owner);

            Assert.AreEqual(20f, skill.GetValue(Armor));

            player.Stats.AddContribution(Health, ContributionType.Flat, 100f, 9u);

            Assert.AreEqual(30f, skill.GetValue(Armor));
        }

        [Test]
        public void Derived_RemoveBySource_Strips() {
            var m = new Modifiable();
            m.Stats.SetBase(Armor, 10f);
            m.Stats.SetBase(Health, 100f);
            m.Stats.AddDerived(Armor, ContributionType.Flat, Health, 0.1f, 1, false, Perspective.Owner, 7u);

            Assert.AreEqual(20f, m.GetValue(Armor));

            m.RemoveSource(7u);

            Assert.AreEqual(10f, m.GetValue(Armor));
        }

        [Test]
        public void Derived_ConditionGates() {
            var m = new Modifiable();
            m.Stats.SetBase(Armor, 10f);
            m.Stats.SetBase(Health, 100f);
            var condition = new StubCondition { Result = false };
            m.Stats.AddDerived(Armor, ContributionType.Flat, Health, 0.1f, 1, false, Perspective.Owner, 7u, condition);

            Assert.AreEqual(10f, m.GetValue(Armor));

            condition.Result = true;

            Assert.AreEqual(20f, m.GetValue(Armor));
        }

        [Test]
        public void Derived_CycleLogsAndSkips() {
            using (new LogMute()) {
                var m = new Modifiable();
                m.Stats.SetBase(Armor, 10f);
                m.Stats.SetBase(Health, 100f);
                m.Stats.AddDerived(Armor, ContributionType.Flat, Health, 1f, 1, false, Perspective.Owner);
                m.Stats.AddDerived(Health, ContributionType.Flat, Armor, 1f, 1, false, Perspective.Owner);

                float armor = 0f, health = 0f;

                Assert.DoesNotThrow(() => armor = m.GetValue(Armor));
                Assert.DoesNotThrow(() => health = m.GetValue(Health));
                Assert.IsFalse(float.IsNaN(armor) || float.IsInfinity(armor));
                Assert.IsFalse(float.IsNaN(health) || float.IsInfinity(health));
            }
        }

        [Test]
        public void Derived_SelfSourceRejected() {
            using (new LogMute()) {
                var m = new Modifiable();
                m.Stats.SetBase(Armor, 10f);
                m.Stats.AddDerived(Armor, ContributionType.Flat, Armor, 1f, 1, false, Perspective.Owner);

                Assert.AreEqual(10f, m.GetValue(Armor));
            }
        }

        [Test]
        public void RolledModifier_AppliesAndRemovesDerivedRows() {
            var armorDef = StatRegistry.GetDefinition("sample_armor");
            var healthDef = StatRegistry.GetDefinition("sample_health");
            var definition = Definitions.Create(
                Definitions.Range(armorDef, ContributionType.Flat, 0.05f, 0.05f, sourceStat: healthDef));

            var target = new Modifiable();
            target.Stats.SetBase(Armor, 10f);
            target.Stats.SetBase(Health, 200f);

            var rolled = definition.Roll(new System.Random(7), 42u);
            rolled.ApplyTo(target, definition);

            Assert.AreEqual(20f, target.GetValue(Armor), 0.001f);

            rolled.RemoveFrom(target);

            Assert.AreEqual(10f, target.GetValue(Armor), 0.001f);
        }

        [Test]
        public void Derived_WorksAsTimedModifier() {
            var target = new Modifiable();
            target.Stats.SetBase(Armor, 10f);
            target.Stats.SetBase(Health, 200f);
            var buffs = new TimedModifierSet(target);

            var definition = ModifierRegistry.GetDefinition("sample_bulwark");
            var rolled = definition.Roll(new System.Random(7), 42u);
            buffs.Apply(rolled, 5f);

            Assert.AreEqual(10f + rolled.baked[0].value * 200f, target.GetValue(Armor), 0.001f);

            buffs.Tick(5.1f);

            Assert.AreEqual(10f, target.GetValue(Armor), 0.001f);
        }
    }
}
