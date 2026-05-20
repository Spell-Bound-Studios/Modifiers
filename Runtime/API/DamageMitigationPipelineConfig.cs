// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// The global damage-mitigation pipeline config. One asset, authored once, activated at startup by
    /// <c>PipelineConfigLoader</c>. Every <c>DamageableModule</c> (and any other slice-based damage
    /// consumer) runs incoming damage through this config's stages.
    /// </summary>
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Damage Mitigation Pipeline Config")]
    public sealed class DamageMitigationPipelineConfig : PipelineConfig<DamageMitigationContext> { }
}