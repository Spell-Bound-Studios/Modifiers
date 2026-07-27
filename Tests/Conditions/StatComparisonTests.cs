// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class StatComparisonTests {
        private static readonly StatId Strength = new(1u);
        private static readonly StatId Dexterity = new(2u);
        private static readonly StatId Armor = new(3u);

        private static Modifiable WithStats(float strength, float dexterity) {
            var modifiable = new Modifiable();
            modifiable.Stats.SetBase(Strength, strength);
            modifiable.Stats.SetBase(Dexterity, dexterity);

            return modifiable;
        }

        [Test]
        public void AtLeast_MetAtBoundary() {
            var ctx = new CircuitContext { Subject = WithStats(10f, 10f) };

            Assert.IsTrue(new StatComparison(Strength, Perspective.Owner, ComparisonOperator.AtLeast, Dexterity,
                    Perspective.Owner).Met(ctx));
        }

        [Test]
        public void GreaterThan_NotMetAtBoundary() {
            var ctx = new CircuitContext { Subject = WithStats(10f, 10f) };

            Assert.IsFalse(new StatComparison(Strength, Perspective.Owner, ComparisonOperator.GreaterThan, Dexterity,
                    Perspective.Owner).Met(ctx));
        }

        [Test]
        public void AtMost_MetAtBoundary_LessThanIsNot() {
            var ctx = new CircuitContext { Subject = WithStats(10f, 10f) };

            Assert.IsTrue(new StatComparison(Strength, Perspective.Owner, ComparisonOperator.AtMost, Dexterity,
                    Perspective.Owner).Met(ctx));
            Assert.IsFalse(new StatComparison(Strength, Perspective.Owner, ComparisonOperator.LessThan, Dexterity,
                    Perspective.Owner).Met(ctx));
        }

        [Test]
        public void Offset_ShiftsRightSide() {
            var ctx = new CircuitContext { Subject = WithStats(15f, 10f) };

            Assert.IsFalse(new StatComparison(Strength, Perspective.Owner, ComparisonOperator.AtLeast, Dexterity,
                    Perspective.Owner, 10f).Met(ctx));
            Assert.IsTrue(new StatComparison(Strength, Perspective.Owner, ComparisonOperator.AtLeast, Dexterity,
                    Perspective.Owner, 5f).Met(ctx));
        }

        [Test]
        public void Perspectives_ReadTheirOwnEntities() {
            var skill = WithStats(5f, 0f);
            var player = WithStats(10f, 0f);
            var ctx = new CircuitContext { Subject = skill, Owner = player };

            Assert.IsFalse(new StatComparison(Strength, Perspective.Subject, ComparisonOperator.AtLeast, Strength,
                    Perspective.Owner).Met(ctx));
            Assert.IsTrue(new StatComparison(Strength, Perspective.Owner, ComparisonOperator.AtLeast, Strength,
                    Perspective.Subject).Met(ctx));
        }

        [Test]
        public void EmptyContext_IsFalse() =>
                Assert.IsFalse(new StatComparison(Strength, Perspective.Owner, ComparisonOperator.AtLeast, Dexterity,
                        Perspective.Owner).Met(new CircuitContext()));

        [Test]
        public void ConditionalContribution_AppliesWhileMet() {
            var modifiable = WithStats(10f, 5f);
            modifiable.Stats.SetBase(Armor, 100f);

            modifiable.Stats.AddContribution(Armor, ContributionType.Increased, 1f, 5u,
                new StatComparison(Strength, Perspective.Owner, ComparisonOperator.AtLeast, Dexterity,
                        Perspective.Owner));

            Assert.AreEqual(200f, modifiable.GetValue(Armor));

            modifiable.Stats.SetBase(Dexterity, 20f);

            Assert.AreEqual(100f, modifiable.GetValue(Armor));
        }
    }
}
