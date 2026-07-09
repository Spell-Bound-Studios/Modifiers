// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Modifiers {
    [Serializable]
    [SerializeReferenceLabel("Named Modifier")]
    public sealed class NamedModifierGrant : ModifierGrant {
        [SerializeField, Tooltip("A ModifierDefinition asset. Its hash makes the roll traceable back to it.")]
        private ModifierDefinition definition;

        public ModifierDefinition Definition => definition;

        public override bool IsValid => definition != null;

        public override void Roll(
            System.Random rng, uint sourceId, List<BakedRoll> baked, List<RolledModifier> modifiers) =>
                modifiers.Add(definition.Roll(rng, sourceId));

        public override void Apply(Modifiable target, uint sourceId, in RolledGrants rolled) {
            if (rolled.modifiers != null) {
                for (var i = 0; i < rolled.modifiers.Length; i++) {
                    if (rolled.modifiers[i].modifierHash != definition.Hash)
                        continue;

                    rolled.modifiers[i].ApplyTo(target, definition);

                    return;
                }
            }

            Log.Warn($"NamedModifierGrant: no rolled record for '{definition.ModifierName}'; skipped. " +
                     "Re-roll the owning instance to include it.");
        }
    }
}
