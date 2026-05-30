// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored ScriptableObject representing one named, player-recognizable identity —
    /// "Iron Will", "Thick Hide", "Fire Resistant", etc. Carries display data (name, icon,
    /// description) plus an embedded <see cref="SbModifier"/> effect that's cloned-and-applied
    /// whenever this trait reaches a target.
    /// </summary>
    /// <remarks>
    /// <para><b>Tiered identities</b> live as separate assets that share the same DisplayName but
    /// differ in icon / description / effect parameters: <c>iron_will_t1.asset</c>,
    /// <c>iron_will_t2.asset</c>, <c>iron_will_t3.asset</c>. The shared C# class
    /// (<c>IronWillModifier</c>) exposes configurable parameters; each Trait asset tunes them.</para>
    /// <para><b>Discovery</b>: assets under <c>Resources/Traits/</c> are indexed at startup by
    /// <see cref="TraitRegistry"/>. The registry key is <see cref="Key"/>'s FNV-1a 32-bit hash,
    /// stable across processes and machines. Save / wire payloads pack the 4-byte id and resolve
    /// back to the asset via the registry.</para>
    /// </remarks>
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Trait")]
    public class Trait : ScriptableObject {
        [Tooltip("Stable registry key, snake_case (e.g. \"iron_will_t1\"). Hashed to a uint for " +
                 "compact save / wire encoding. Must be unique across all Trait assets."), SerializeField]
        private string key;

        [Tooltip("Player-facing display name shown on nameplates and tooltips (e.g. \"Iron Will\"). " +
                 "Multiple tiers of the same identity typically share the DisplayName but differ " +
                 "in Icon and Effect tuning."), SerializeField]
        private string displayName;

        [Tooltip("Icon shown on nameplates / tooltips. Tier variants typically use copper / silver " +
                 "/ gold colorations to communicate strength at a glance."), SerializeField]
        private Sprite icon;

        [Tooltip("Tooltip / nameplate hover description."), SerializeField, TextArea(2, 5)]
        private string description;

        [Tooltip("The actual SbModifier behavior cloned-and-applied when this Trait reaches a " +
                 "target. Configure the behavior's parameters here per-tier."), SerializeReference, DropdownPicker]
        private SbModifier effect;

        public string Key => key;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? key : displayName;
        public Sprite Icon => icon;
        public string Description => description;
        public SbModifier Effect => effect;
    }
}