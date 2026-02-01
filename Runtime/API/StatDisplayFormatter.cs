using Spellbound.Core;
using UnityEngine;

namespace Spellbound.Stats {
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Stat Display Format")]
    public class StatDisplayFormat : ScriptableObject {
        [Header("Format")]
        [SerializeField, Tooltip("Text before the value (e.g., '+', '$')")]
        private string prefix = "";
        
        [SerializeField, Tooltip("Text after the value (e.g., '%', 's', 'm')")]
        private string suffix = "";
        
        [SerializeField, Tooltip("Decimal places to show (0 for integers)")]
        private int decimalPlaces;
        
        [Header("Preview")]
        [SerializeField, Immutable] private string example;
        
        public string Prefix => prefix;
        public string Suffix => suffix;
        public int DecimalPlaces => decimalPlaces;
        
        public string Format(float value) {
            var format = decimalPlaces > 0 ? $"F{decimalPlaces}" : "F0";
            return $"{prefix}{value.ToString(format)}{suffix}";
        }
        
#if UNITY_EDITOR
        private void OnValidate() {
            example = Format(150.55f);
        }
#endif
    }
}