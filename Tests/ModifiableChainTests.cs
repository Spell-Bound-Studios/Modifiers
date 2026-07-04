// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class ModifiableChainTests {
        private static readonly StatId Fire = new(1u);
        private static readonly StatId Armor = new(2u);
        private static readonly StatId Strength = new(3u);

        [Test]
        public void GetValue_MergesLayersIntoOneCalculation() {
            var player = new Modifiable();
            var fireball = new Modifiable { Parent = player };

            fireball.Stats.SetBase(Fire, 30f);
            fireball.Stats.AddModifier(Fire, ModifierType.Increased, 0.2f);
            fireball.Stats.AddModifier(Fire, ModifierType.More, 0.25f);

            player.Stats.AddModifier(Fire, ModifierType.Flat, 5f);
            player.Stats.AddModifier(Fire, ModifierType.Increased, 0.3f);
            player.Stats.AddModifier(Fire, ModifierType.More, 0.1f);

            Assert.AreEqual(72.1875f, fireball.GetValue(Fire));
        }

        [Test]
        public void GetValue_TwoSkillsShareParentButStayPartitioned() {
            var player = new Modifiable();
            var fireball = new Modifiable { Parent = player };
            var flamethrower = new Modifiable { Parent = player };

            fireball.Stats.SetBase(Fire, 30f);
            flamethrower.Stats.SetBase(Fire, 20f);
            flamethrower.Stats.AddModifier(Fire, ModifierType.Flat, 10f);

            player.Stats.AddModifier(Fire, ModifierType.Flat, 5f);

            Assert.AreEqual(35f, fireball.GetValue(Fire));
            Assert.AreEqual(35f, flamethrower.GetValue(Fire));
        }

        [Test]
        public void GetValue_ParentBaseUsedWhenChildHasNone() {
            var player = new Modifiable();
            var skill = new Modifiable { Parent = player };

            player.Stats.SetBase(Armor, 10f);

            Assert.AreEqual(10f, skill.GetValue(Armor));
        }

        [Test]
        public void GetValue_ChildZeroBaseBeatsParentBase() {
            var player = new Modifiable();
            var skill = new Modifiable { Parent = player };

            player.Stats.SetBase(Armor, 50f);
            skill.Stats.SetBase(Armor, 0f);

            Assert.AreEqual(0f, skill.GetValue(Armor));
        }

        [Test]
        public void GetValue_OverrideAcrossLayers_LowestWins() {
            var player = new Modifiable();
            var skill = new Modifiable { Parent = player };
            skill.Stats.SetBase(Fire, 30f);

            player.Stats.AddModifier(Fire, ModifierType.Override, 10f);
            skill.Stats.AddModifier(Fire, ModifierType.Override, 3f);

            Assert.AreEqual(3f, skill.GetValue(Fire));
        }

        [Test]
        public void RemoveSource_OnParent_ReachesEveryReader() {
            var player = new Modifiable();
            var fireball = new Modifiable { Parent = player };
            var flamethrower = new Modifiable { Parent = player };

            fireball.Stats.SetBase(Fire, 30f);
            flamethrower.Stats.SetBase(Fire, 20f);

            const uint helmet = 77u;
            player.Stats.AddModifier(Armor, ModifierType.Flat, 5f, helmet);
            player.Stats.AddModifier(Fire, ModifierType.Flat, 5f, helmet);

            Assert.AreEqual(35f, fireball.GetValue(Fire));
            Assert.AreEqual(25f, flamethrower.GetValue(Fire));

            player.RemoveSource(helmet);

            Assert.AreEqual(30f, fireball.GetValue(Fire));
            Assert.AreEqual(20f, flamethrower.GetValue(Fire));
            Assert.AreEqual(0f, player.GetValue(Armor));
        }

        [Test]
        public void GetValue_ConditionalOnChildReadsThroughChain() {
            var player = new Modifiable();
            var skill = new Modifiable { Parent = player };

            player.Stats.SetBase(Strength, 50f);
            skill.Stats.SetBase(Fire, 30f);
            skill.Stats.AddModifier(Fire, ModifierType.Increased, 1f, 5u, new StatAtLeast(Strength, 50f));

            Assert.AreEqual(60f, skill.GetValue(Fire));

            player.Stats.SetBase(Strength, 10f);

            Assert.AreEqual(30f, skill.GetValue(Fire));
        }

        [Test]
        public void GetValue_ParentConditional_EvaluatesAgainstParentNotQuerier() {
            var character = new Modifiable();
            var skill = new Modifiable { Parent = character };

            character.Stats.SetBase(Strength, 100f);
            character.Stats.AddModifier(Fire, ModifierType.Increased, 0.4f, 5u, new StatAtLeast(Strength, 100f));

            skill.Stats.SetBase(Fire, 30f);
            skill.Stats.SetBase(Strength, 0f);

            Assert.AreEqual(42f, skill.GetValue(Fire));
        }

        [Test]
        public void GetValue_EachBlocksConditional_SeesItsOwnOwner() {
            var character = new Modifiable();
            var skill = new Modifiable { Parent = character };

            character.Stats.SetBase(Strength, 100f);
            character.Stats.AddModifier(Fire, ModifierType.Increased, 0.5f, 5u, new StatAtLeast(Strength, 100f));

            skill.Stats.SetBase(Fire, 30f);
            skill.Stats.SetBase(Strength, 0f);
            skill.Stats.AddModifier(Fire, ModifierType.Increased, 0.5f, 6u, new StatAtLeast(Strength, 50f));

            Assert.AreEqual(45f, skill.GetValue(Fire));
        }

        [Test]
        public void Parent_CycleRejected() {
            var a = new Modifiable();
            var b = new Modifiable();
            b.Parent = a;

            using (new LogMute()) {
                a.Parent = b;
                a.Parent = a;
            }

            Assert.IsNull(a.Parent);
            Assert.AreSame(a, b.Parent);
        }
    }
}
