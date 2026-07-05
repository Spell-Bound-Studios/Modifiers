// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class StatTemplateTests {
        private static StatId Armor => new(StatRegistry.GetHash("sample_armor"));
        private static StatId Health => new(StatRegistry.GetHash("sample_health"));

        private static StatTemplate.BaseStat Base(string statName, float value) =>
                new() { stat = StatRegistry.GetDefinition(statName), value = value };

        [Test]
        public void ApplyTo_SetsBases() {
            var template = Definitions.CreateTemplate(new[] {
                Base("sample_armor", 10f),
                Base("sample_health", 120f)
            });

            var target = new Modifiable();
            template.ApplyTo(target);

            Assert.AreEqual(10f, target.GetValue(Armor));
            Assert.AreEqual(120f, target.GetValue(Health));
        }

        [Test]
        public void ApplyTo_SkipsNullStats() {
            var template = Definitions.CreateTemplate(new[] {
                new StatTemplate.BaseStat { stat = null, value = 5f },
                Base("sample_armor", 10f)
            });

            var target = new Modifiable();

            Assert.DoesNotThrow(() => template.ApplyTo(target));
            Assert.AreEqual(10f, target.GetValue(Armor));
        }

        [Test]
        public void RollInnate_RollsEachDefinition() {
            var thickHide = ModifierRegistry.GetDefinition("thick_hide");
            var vigorous = ModifierRegistry.GetDefinition("vigorous");
            var template = Definitions.CreateTemplate(new StatTemplate.BaseStat[0], thickHide, vigorous);

            var rolled = template.RollInnate(new System.Random(7));

            Assert.AreEqual(2, rolled.Count);
            Assert.AreEqual(thickHide.Hash, rolled[0].modifierHash);
            Assert.AreEqual(vigorous.Hash, rolled[1].modifierHash);
            Assert.AreNotEqual(0u, rolled[0].sourceId);
            Assert.AreNotEqual(0u, rolled[1].sourceId);
        }

        [Test]
        public void RollInnate_SkipsNullDefinitions() {
            var template = Definitions.CreateTemplate(new StatTemplate.BaseStat[0],
                null, ModifierRegistry.GetDefinition("thick_hide"));

            Assert.AreEqual(1, template.RollInnate(new System.Random(7)).Count);
        }

        [Test]
        public void RollInnate_SameSeed_SameRolls() {
            var template = Definitions.CreateTemplate(new StatTemplate.BaseStat[0],
                ModifierRegistry.GetDefinition("thick_hide"));

            var first = template.RollInnate(new System.Random(42));
            var second = template.RollInnate(new System.Random(42));

            Assert.AreEqual(first[0].values[0], second[0].values[0]);
            Assert.AreEqual(first[0].sourceId, second[0].sourceId);
        }

        [Test]
        public void SpawnFlow_BasesPlusInnates_Compose() {
            var template = Definitions.CreateTemplate(new[] { Base("sample_armor", 10f) },
                ModifierRegistry.GetDefinition("thick_hide"));

            var target = new Modifiable();
            template.ApplyTo(target);

            var rolled = template.RollInnate(new System.Random(7));

            foreach (var modifier in rolled)
                modifier.TryApplyTo(target);

            Assert.AreEqual(10f + rolled[0].values[0], target.GetValue(Armor), 0.001f);

            target.RemoveSource(rolled[0].sourceId);

            Assert.AreEqual(10f, target.GetValue(Armor), 0.001f);
        }
    }
}
