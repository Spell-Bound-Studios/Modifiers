// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;
using UnityEngine;

namespace Spellbound.Modifiers.Tests {
    public class ModifierDefinitionTests {
        [Test]
        public void Roll_ValuesStayWithinRange() {
            var stat = ScriptableObject.CreateInstance<StatDefinition>();
            var definition = Definitions.Create(Definitions.Range(stat, ContributionType.Flat, 8f, 15f));
            var rng = new System.Random(1234);

            for (var i = 0; i < 50; i++) {
                var rolled = definition.Roll(rng, 1u);

                Assert.GreaterOrEqual(rolled.values[0], 8f);
                Assert.LessOrEqual(rolled.values[0], 15f);
            }
        }

        [Test]
        public void Roll_StepOne_RollsIntegers() {
            var stat = ScriptableObject.CreateInstance<StatDefinition>();
            var definition = Definitions.Create(Definitions.Range(stat, ContributionType.Flat, 1f, 9f, 1f));
            var rng = new System.Random(1234);

            for (var i = 0; i < 50; i++) {
                var value = definition.Roll(rng, 1u).values[0];

                Assert.AreEqual(Mathf.Round(value), value);
                Assert.GreaterOrEqual(value, 1f);
                Assert.LessOrEqual(value, 9f);
            }
        }

        [Test]
        public void Roll_ValueCountMatchesContributions() {
            var stat = ScriptableObject.CreateInstance<StatDefinition>();

            var definition = Definitions.Create(
                Definitions.Range(stat, ContributionType.Flat, 1f, 2f),
                Definitions.Range(stat, ContributionType.Increased, 0.1f, 0.2f));

            Assert.AreEqual(2, definition.Roll(new System.Random(1), 1u).values.Length);
        }

        [Test]
        public void Roll_CarriesSourceIdAndHash() {
            var stat = ScriptableObject.CreateInstance<StatDefinition>();
            var definition = Definitions.Create(Definitions.Range(stat, ContributionType.Flat, 1f, 2f));

            var rolled = definition.Roll(new System.Random(1), 42u);

            Assert.AreEqual(42u, rolled.sourceId);
            Assert.AreEqual(definition.Hash, rolled.modifierHash);
        }

        [Test]
        public void ApplyTo_AddsContributions_RemoveFromStrips() {
            var stat = ScriptableObject.CreateInstance<StatDefinition>();
            var statId = new StatId(stat.Hash);
            var definition = Definitions.Create(Definitions.Range(stat, ContributionType.Flat, 10f, 10f));
            var target = new Modifiable();
            target.Stats.SetBase(statId, 100f);

            var rolled = definition.Roll(new System.Random(1), 42u);
            rolled.ApplyTo(target, definition);

            Assert.AreEqual(110f, target.GetValue(statId));

            rolled.RemoveFrom(target);

            Assert.AreEqual(100f, target.GetValue(statId));
        }
    }
}