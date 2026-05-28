// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Tooling;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored asset declaring one stat: its name (the registry key), human-readable display name,
    /// description, icon, and an optional <see cref="StatDisplayFormat"/> for UI formatting. Aggregated into
    /// a <see cref="StatDatabase"/> at the project level; referenced directly by preset modules via
    /// <see cref="StatBaseEntry"/> / <see cref="ModifierEntry"/> / <see cref="ResourceBaseEntry"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="OnValidate"/> re-formats a preview value (150.55) so designers see what their display
    /// format does without entering Play mode.
    /// </remarks>
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Stat Definition")]
    public class StatDefinition : ScriptableObject {
        [Header("Identity"), SerializeField] private string statName;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField, SpritePreview] private Sprite icon;

        [Header("Display"), SerializeField, DropdownPicker]
        private StatDisplayFormat displayFormat;

        [Header("Preview (Example Value: 150.55)"), SerializeField, Immutable]
        private string internalStorage;

        [SerializeField, Immutable] private string formattedDisplay;

        public string StatName => statName;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? statName : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public StatDisplayFormat DisplayFormat => displayFormat;

        public int Register() {
            StatDefinitionRegistry.Register(this);

            return StatRegistry.Register(statName);
        }

        public string FormatValue(float value) =>
                displayFormat != null
                        ? displayFormat.Format(value)
                        : value.ToString("F0");

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