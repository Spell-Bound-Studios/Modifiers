// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// The one authored list for putting modifiers on a thing. Each grant is a named
    /// <see cref="ModifierDefinition"/> or an inline contribution, chosen per entry in the inspector.
    /// <see cref="Roll"/> once at the owning instance's creation, persist the <see cref="RolledGrants"/> with
    /// that instance, and hydrate back through <see cref="Apply"/>; or <see cref="RollAndApply"/> for
    /// fire-and-forget. Everything lands under one source id, so RemoveSource strips the whole set.
    /// Apply's sourceId is authoritative: named records are re-keyed to it before applying; a record's
    /// packed id matters only when applied standalone (StatData, TimedModifierSet).
    /// </summary>
    [Serializable]
    public sealed class ModifierGrantSet {
        [SerializeReference] private List<ModifierGrant> grants = new();

        public IReadOnlyList<ModifierGrant> Grants => grants;

        public RolledGrants Roll(System.Random rng, uint sourceId) {
            var baked = new List<BakedRoll>();
            var modifiers = new List<RolledModifier>();

            for (var i = 0; i < grants.Count; i++) {
                var grant = grants[i];

                if (grant == null || !grant.IsValid) {
                    Log.Warn($"ModifierGrantSet: grant {i} is invalid (needs a definition, or a stat and " +
                             "magnitude); skipped.");

                    continue;
                }

                grant.Roll(rng, sourceId, baked, modifiers);
            }

            var seenStats = new HashSet<uint>();

            for (var i = 0; i < baked.Count; i++) {
                if (seenStats.Add(baked[i].statHash))
                    continue;

                Log.Error($"ModifierGrantSet: two rolled contributions on stat '{new StatId(baked[i].statHash)}'. " +
                          "Rolled values are keyed by stat — keeping the first roll; give each a distinct stat.");
                baked.RemoveAt(i--);
            }

            var seenModifiers = new HashSet<uint>();

            for (var i = 0; i < modifiers.Count; i++) {
                if (seenModifiers.Add(modifiers[i].modifierHash))
                    continue;

                Log.Error($"ModifierGrantSet: modifier '{modifiers[i]}' is granted twice. Rolled records are " +
                          "keyed by modifier hash — keeping the first roll; grant each definition once.");
                modifiers.RemoveAt(i--);
            }

            return new RolledGrants { baked = baked.ToArray(), modifiers = modifiers.ToArray() };
        }

        public void Apply(Modifiable target, uint sourceId, in RolledGrants rolled) {
            for (var i = 0; i < grants.Count; i++) {
                var grant = grants[i];

                if (grant == null || !grant.IsValid) {
                    Log.Warn($"ModifierGrantSet: grant {i} is invalid (needs a definition, or a stat and " +
                             "magnitude); skipped.");

                    continue;
                }

                grant.Apply(target, sourceId, in rolled);
            }
        }

        public void RollAndApply(Modifiable target, System.Random rng, uint sourceId) =>
                Apply(target, sourceId, Roll(rng, sourceId));
    }
}
