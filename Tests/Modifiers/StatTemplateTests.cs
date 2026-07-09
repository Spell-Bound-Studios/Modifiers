// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class StatTemplateTests {
        private static StatId Armor => new(StatRegistry.GetHash("sample_armor"));
        private static StatId Health => new(StatRegistry.GetHash("sample_health"));

        private static BaseStat Base(string statName, float value) =>
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
            using (new LogMute()) {
                var template = Definitions.CreateTemplate(new[] {
                    new BaseStat { stat = null, value = 5f },
                    Base("sample_armor", 10f)
                });

                var target = new Modifiable();

                Assert.DoesNotThrow(() => template.ApplyTo(target));
                Assert.AreEqual(10f, target.GetValue(Armor));
            }
        }

        [Test]
        public void Modifiers_ExposesAuthoredDefinitions() {
            var thickHide = ModifierRegistry.GetDefinition("sample_thick_hide");
            var vigorous = ModifierRegistry.GetDefinition("sample_vigorous");
            var template = Definitions.CreateTemplate(new BaseStat[0], thickHide, vigorous);

            Assert.AreEqual(2, template.Modifiers.Count);
            Assert.AreEqual(thickHide, template.Modifiers[0]);
            Assert.AreEqual(vigorous, template.Modifiers[1]);
        }

        [Test]
        public void SpawnFlow_BasesPlusModifiers_Compose() {
            var template = Definitions.CreateTemplate(new[] { Base("sample_armor", 10f) },
                ModifierRegistry.GetDefinition("sample_thick_hide"));

            var target = new Modifiable();
            template.ApplyTo(target);

            var rolled = template.Modifiers[0].Roll(new System.Random(7), 7u);
            rolled.TryApplyTo(target);

            Assert.AreEqual(10f + rolled.baked[0].value, target.GetValue(Armor), 0.001f);

            target.RemoveSource(7u);

            Assert.AreEqual(10f, target.GetValue(Armor), 0.001f);
        }
    }
}
