// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using System.Text;
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
        public static readonly Color Chaos = new(0.75f, 0.35f, 0.9f);     // violet
        public static readonly Color Absorb = new(0.95f, 0.78f, 0.2f);    // amber (shield, not mana)
        public static readonly Color Deposit = new(0.85f, 0.2f, 0.2f);    // red
        public static readonly Color Neutral = new(0.3f, 0.34f, 0.42f);

        private static uint? _fire;
        private static uint? _physical;
        private static uint? _cold;
        private static uint? _lightning;
        private static uint? _chaos;

        /// <summary>
        /// The palette color for a damage-type stat, or white for anything unrecognized.
        /// </summary>
        public static Color ForDamage(uint statHash) {
            _fire ??= StatRegistry.GetHash("sample_fire_damage");

            if (statHash == _fire)
                return Fire;

            _physical ??= StatRegistry.GetHash("sample_physical_damage");

            if (statHash == _physical)
                return Physical;

            _cold ??= StatRegistry.GetHash("sample_cold_damage");

            if (statHash == _cold)
                return Cold;

            _lightning ??= StatRegistry.GetHash("sample_lightning_damage");

            if (statHash == _lightning)
                return Lightning;

            _chaos ??= StatRegistry.GetHash("sample_chaos_damage");

            if (statHash == _chaos)
                return Chaos;

            return Color.white;
        }

        public static Color ForModifier(uint modifierHash) =>
                Color.HSVToRGB(modifierHash % 360u / 360f, 0.65f, 0.95f);

        public static string ModifierIcons(IReadOnlyList<RolledModifier> rolled) {
            if (rolled == null || rolled.Count == 0)
                return "";

            var sb = new StringBuilder(rolled.Count * 24);

            for (var i = 0; i < rolled.Count; i++) {
                var color = ForModifier(rolled[i].modifierHash);
                sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(color)).Append(">■</color>");
            }

            return sb.ToString();
        }

        /// <summary>
        /// The palette color for a circuit node, keyed by its id.
        /// </summary>
        public static Color ForNode(string nodeId) => nodeId switch {
            "absorption" => Absorb,
            "fire" => Fire,
            "cold" => Cold,
            "lightning" => Lightning,
            "armor" => Physical,
            "deposit" => Deposit,
            "killing-blow" => Deposit,
            "reflect-fire" => Fire,
            _ => Neutral
        };
    }
}
