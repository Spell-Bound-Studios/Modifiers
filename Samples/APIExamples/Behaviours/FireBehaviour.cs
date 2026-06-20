// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample behaviour: the fire payload of a skill. Owns <c>fire_damage</c> and builds the typed-damage list
    /// the cast sends at its targets — doubled when empowered by a banked killing blow. <see cref="IgniteEnabled"/>
    /// is a capability the <see cref="IgniteModifier"/> flips on so hits also light the target on fire.
    /// </summary>
    [Serializable]
    public sealed class FireBehaviour : SbBehaviour {
        private static uint? _fireDamageHash;
        private static uint FireDamageHash => _fireDamageHash ??= StatRegistry.GetHash("fire_damage");

        public bool IgniteEnabled { get; set; }

        public override IReadOnlyList<StatAndValue> DeclareOwnedStats() => new[] { OwnedStat("fire_damage", 30f) };

        public float FireDamage => GetValue(FireDamageHash);

        public List<StatAndValue> BuildDamage(bool empowered) {
            var amount = FireDamage * (empowered ? 2f : 1f);

            return new List<StatAndValue> {
                new(FireDamageHash, amount)
            };
        }
    }
}
