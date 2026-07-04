// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Items that flip capabilities on the skill rather than contributing stats. Ignite already graduated to
    /// stats; each of these is a candidate for the same promotion once its numbers want to stack.
    /// </summary>
    public sealed class CircularNovaItem : FireballItem {
        public override void Equip(Fireball fireball) => fireball.DirectionOverride = Circle;

        public override void Unequip(Fireball fireball) => fireball.DirectionOverride = null;

        private static Vector3[] Circle(int count) {
            var directions = new Vector3[count];

            for (var i = 0; i < count; i++)
                directions[i] = Quaternion.AngleAxis(360f / count * i, Vector3.up) * Vector3.forward;

            return directions;
        }
    }

    public sealed class SplitOnHitItem : FireballItem {
        public override void Equip(Fireball fireball) => fireball.SplitOnHit = true;

        public override void Unequip(Fireball fireball) => fireball.SplitOnHit = false;
    }

    public sealed class EmpowerOnKillItem : FireballItem {
        public override void Equip(Fireball fireball) => fireball.EmpowermentEnabled = true;

        public override void Unequip(Fireball fireball) => fireball.EmpowermentEnabled = false;
    }

    public sealed class LifeStealItem : FireballItem {
        private readonly float _fraction;

        public LifeStealItem(float fraction = 0.3f) => _fraction = fraction;

        public override void Equip(Fireball fireball) => fireball.LifeStealFraction = _fraction;

        public override void Unequip(Fireball fireball) => fireball.LifeStealFraction = 0f;
    }
}
