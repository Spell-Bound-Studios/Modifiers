// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers {
    [Serializable]
    public class ModifierGrant {
        [SerializeField] private ModifierDefinition definition;
        [SerializeField] private ContributionRange inline;

        public ModifierDefinition Definition => definition;
        public ContributionRange Inline => inline;

        public bool IsValid => definition != null || inline.stat != null;

        public IRolledModifier Roll(System.Random rng, uint sourceId) =>
                definition != null ? definition.Roll(rng, sourceId) : inline.RollContribution(rng, sourceId);
    }
}
