// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Scaffolding: the demo's one palette. Damage types and circuit nodes share a color language so the floating
    /// numbers and the circuit diagram read together — fire is orange everywhere, etc.
    /// </summary>
    public static class CombatColors {
        public static readonly Color Fire = new(1f, 0.5f, 0.1f);          // orange
        public static readonly Color Cold = new(0.45f, 0.78f, 1f);        // frosty blue
        public static readonly Color Lightning = new(1f, 0.92f, 0.2f);    // yellow
        public static readonly Color Physical = new(0.55f, 0.38f, 0.22f); // brown
        public static readonly Color Absorb = new(0.95f, 0.78f, 0.2f);    // amber (shield, not mana)
        public static readonly Color Deposit = new(0.85f, 0.2f, 0.2f);    // red
        public static readonly Color Neutral = new(0.3f, 0.34f, 0.42f);

        private static uint? _fire;
        private static uint? _physical;
        private static uint? _cold;
        private static uint? _lightning;

        /// <summary>The palette color for a damage-type stat, or white for anything unrecognized.</summary>
        public static Color ForDamage(uint statHash) {
            _fire ??= StatRegistry.GetHash("fire_damage");

            if (statHash == _fire)
                return Fire;

            _physical ??= StatRegistry.GetHash("physical_damage");

            if (statHash == _physical)
                return Physical;

            _cold ??= StatRegistry.GetHash("cold_damage");

            if (statHash == _cold)
                return Cold;

            _lightning ??= StatRegistry.GetHash("lightning_damage");

            if (statHash == _lightning)
                return Lightning;

            return Color.white;
        }

        /// <summary>The palette color for a circuit node, keyed by its id.</summary>
        public static Color ForNode(string nodeId) => nodeId switch {
            "absorption" => Absorb,
            "fire" => Fire,
            "cold" => Cold,
            "lightning" => Lightning,
            "armor" => Physical,
            "deposit" => Deposit,
            "reflect-fire" => Fire,
            _ => Neutral
        };
    }
}
