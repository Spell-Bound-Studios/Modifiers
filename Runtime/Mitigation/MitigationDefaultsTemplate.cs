// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored vanilla mitigation table. One asset per character archetype (or one shared across
    /// all characters); referenced by the game's character controller and baked into each target's
    /// per-instance row list at init. Modifiers edit the baked instance list at runtime — the asset is
    /// never touched.
    /// </summary>
    [CreateAssetMenu(menuName = "Spellbound/Mitigation/Mitigation Defaults Template")]
    public sealed class MitigationDefaultsTemplate : ScriptableObject {
        public List<MitigationRowAuthoring> rows = new();
    }
}
