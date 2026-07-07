// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class ModifierGrantTests {
        private static StatId Armor => new(StatRegistry.GetHash("sample_armor"));

        [Test]
        public void Roll_Named_ProducesRolledModifier_AndApplies() {
            var grant = Definitions.Grant(ModifierRegistry.GetDefinition("sample_thick_hide"));

            var rolled = grant.Roll(new System.Random(7), 11u);

            Assert.IsInstanceOf<RolledModifier>(rolled);

            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            rolled.TryApplyTo(target);

            Assert.Greater(target.GetValue(Armor), 100f);
        }

        [Test]
        public void Roll_Inline_ProducesRolledContribution_AndApplies() {
            var range = Definitions.Range(StatRegistry.GetDefinition("sample_armor"), ContributionType.Flat, 5f, 5f);
            var grant = Definitions.Grant(range);

            var rolled = grant.Roll(new System.Random(7), 11u);

            Assert.IsInstanceOf<RolledContribution>(rolled);

            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            rolled.TryApplyTo(target);

            Assert.AreEqual(105f, target.GetValue(Armor));

            rolled.RemoveFrom(target);

            Assert.AreEqual(100f, target.GetValue(Armor));
        }

        [Test]
        public void IsValid_TrueForEitherSource() {
            Assert.IsTrue(Definitions.Grant(ModifierRegistry.GetDefinition("sample_thick_hide")).IsValid);
            Assert.IsTrue(Definitions.Grant(
                Definitions.Range(StatRegistry.GetDefinition("sample_armor"), ContributionType.Flat, 1f, 2f)).IsValid);
        }

        [Test]
        public void IsValid_FalseWhenEmpty() {
            Assert.IsFalse(new ModifierGrant().IsValid);
        }

        [Test]
        public void MixedGrants_RollIntoOneUniformList() {
            var grants = new[] {
                Definitions.Grant(ModifierRegistry.GetDefinition("sample_thick_hide")),
                Definitions.Grant(Definitions.Range(StatRegistry.GetDefinition("sample_armor"),
                    ContributionType.Flat, 5f, 5f))
            };

            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            var rng = new System.Random(7);

            for (var i = 0; i < grants.Length; i++)
                grants[i].Roll(rng, (uint)(i + 1)).TryApplyTo(target);

            Assert.Greater(target.GetValue(Armor), 105f);
        }
    }
}
