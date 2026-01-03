// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core;
using UnityEngine;

namespace Spellbound.Stats {
    [Serializable]
    public class ModifierModule : PresetModule {
        // This represents a collection of modifiers that are given and not crafted/rolled/etc. AKA Cruel Barb.
        [SerializeReference] private List<IModifier> fixedMods = new();
        // Intended to be the number of open/available modifiers on a Modifier.
        [SerializeField] private int numberOfMods;
        
        public override SbbData? GetData(ObjectPreset preset) => throw new System.NotImplementedException();
    }
}