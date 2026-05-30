// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Inspector-side form of <see cref="MitigationRow"/>. Designers reference <see cref="StatDefinition"/>
    /// assets so renames flow through the asset graph; the picker on <see cref="strategy"/> lists every
    /// concrete <see cref="IMitigationStrategy"/> in the project (game-side implementations included).
    /// Baked to a runtime <see cref="MitigationRow"/> at character init.
    /// </summary>
    [Serializable]
    public class MitigationRowAuthoring {
        public StatDefinition defenseStat;
        public List<StatDefinition> coveredDamageStats = new();

        [SerializeReference, DropdownPicker] public IMitigationStrategy strategy;
    }
}