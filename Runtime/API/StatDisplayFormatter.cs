// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Tooling;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored UI formatter for a stat value: prefix (e.g. <c>+</c>, <c>$</c>) + body
    /// (<c>F0</c> / <c>F1</c> / …) + suffix (e.g. <c>%</c>, <c>s</c>, <c>m</c>). Assigned to one or more
    /// <see cref="StatDefinition"/> assets so different stats can share a presentation style (all percent
    /// resists, all flat damages, etc.).
    /// </summary>
    /// <remarks>
    /// File name is <c>StatDisplayFormatter.cs</c> but the type is <c>StatDisplayFormat</c> — worth renaming
    /// one to match the other when the rest of the lib gets a 1.0 pass.
    /// </remarks>
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Stat Display Format")]
    public class StatDisplayFormat : ScriptableObject {
        [Header("Format"), SerializeField, Tooltip("Text before the value (e.g., '+', '$')")]
        private string prefix = "";

        [SerializeField, Tooltip("Text after the value (e.g., '%', 's', 'm')")]
        private string suffix = "";

        [SerializeField, Tooltip("Decimal places to show (0 for integers)")]
        private int decimalPlaces;

        [Header("Preview"), SerializeField, Immutable]
        private string example;

        public string Prefix => prefix;
        public string Suffix => suffix;
        public int DecimalPlaces => decimalPlaces;

        public string Format(float value) {
            var format = decimalPlaces > 0 ? $"F{decimalPlaces}" : "F0";

            return $"{prefix}{value.ToString(format)}{suffix}";
        }

#if UNITY_EDITOR
        private void OnValidate() => example = Format(150.55f);
#endif
    }
}