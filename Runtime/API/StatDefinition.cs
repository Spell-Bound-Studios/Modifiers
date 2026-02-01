using Spellbound.Core;
using UnityEngine;

namespace Spellbound.Stats {
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Stat Definition")]
    public class StatDefinition : ScriptableObject {
        [Header("Identity")]
        [SerializeField] private string statName;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField, SpritePreview] private Sprite icon;
        
        [Header("Display")]
        [SerializeField, DropdownPicker] private StatDisplayFormat displayFormat;
        
        [Header("Preview (Example Value: 150.55)")]
        [SerializeField, Immutable] private string internalStorage;
        [SerializeField, Immutable] private string formattedDisplay;
    
        public string StatName => statName;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? statName : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public StatDisplayFormat DisplayFormat => displayFormat;
    
        public int Register() => StatRegistry.Register(statName);
        
        public string FormatValue(float value) {
            return displayFormat != null 
                ? displayFormat.Format(value) 
                : value.ToString("F0");
        }
        
#if UNITY_EDITOR
        private const float PreviewValue = 150.55f;
        
        private void OnValidate() {
            var internalValue = StatSettings.ToInternal(PreviewValue);
            
            internalStorage = $"{internalValue} (precision: {StatSettings.Precision})";
            formattedDisplay = FormatValue(StatSettings.ToExternal(internalValue));
        }
#endif
    }
}