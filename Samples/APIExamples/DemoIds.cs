// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Hashing;

namespace Spellbound.Modifiers.Samples {
    public static class DemoStats {
        public static readonly StatId Health = StatId.From("sample_health");
        public static readonly StatId Shield = StatId.From("sample_shield");
        public static readonly StatId KillingBlow = StatId.From("sample_killing_blow");
        public static readonly StatId Armor = StatId.From("sample_armor");
        public static readonly StatId FireResistance = StatId.From("sample_fire_resistance");
        public static readonly StatId ColdResistance = StatId.From("sample_cold_resistance");
        public static readonly StatId LightningResistance = StatId.From("sample_lightning_resistance");
        public static readonly StatId PhysicalDamage = StatId.From("sample_physical_damage");
        public static readonly StatId FireDamage = StatId.From("sample_fire_damage");
        public static readonly StatId ColdDamage = StatId.From("sample_cold_damage");
        public static readonly StatId LightningDamage = StatId.From("sample_lightning_damage");
        public static readonly StatId ProjectileCount = StatId.From("sample_projectile_count");
        public static readonly StatId ProjectileSpeed = StatId.From("sample_projectile_speed");
        public static readonly StatId IgniteChance = StatId.From("sample_ignite_chance");
        public static readonly StatId IgniteDuration = StatId.From("sample_ignite_duration");
    }

    /// <summary>
    /// Short display names for demo stats — "sample_fire_damage" reads as "fire" in diagrams and labels.
    /// </summary>
    public static class DemoNames {
        public static string Short(StatId stat) =>
                stat.ToString()
                    .Replace("sample_", string.Empty)
                    .Replace("_damage", string.Empty)
                    .Replace("_resistance", string.Empty)
                    .Replace('_', ' ');
    }

    /// <summary>
    /// Consequence vocabulary: the ids an evaluation reports outcomes under on the context's consequence list.
    /// A game may key them by registered stats (the killing blow reuses its stat hash, buying a display name)
    /// or by bare name hashes (reflected fire) — the channel doesn't care.
    /// </summary>
    public static class DemoConsequences {
        public static readonly uint KillingBlow = DemoStats.KillingBlow;
        public static readonly uint ReflectedFire = StableHash.Fnv1A32("reflected_fire");
    }

    public static class DemoEvents {
        public static readonly uint TakeHit = StableHash.Fnv1A32("take_hit");
    }

    public static class DemoStages {
        public static readonly uint Convert = StableHash.Fnv1A32("convert");
        public static readonly uint Mitigate = StableHash.Fnv1A32("mitigate");
        public static readonly uint Apply = StableHash.Fnv1A32("apply");
        public static readonly uint React = StableHash.Fnv1A32("react");
    }
}
