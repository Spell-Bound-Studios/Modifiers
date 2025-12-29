// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Example behavioral modifier: Gain a Frenzy Charge on kill.
    /// Demonstrates how modifiers can subscribe to skill events.
    /// </summary>
    public class OnKillFrenzyChargeModifier : IModifier {
        public int ModifierId { get; }
        public HashSet<int> RequiredTags { get; }

        private int _frenzyCharges = 0;

        public OnKillFrenzyChargeModifier(int modifierId, HashSet<int> requiredTags) {
            ModifierId = modifierId;
            RequiredTags = requiredTags;
        }

        public void Apply(IModifiable target) {
            // Check if target has matching tags
            if (!HasMatchingTags(target))
                return;

            // Subscribe to OnKill event if the skill has it
            if (target is not FireballSkill fireball) 
                return;

            fireball.OnKill += HandleKill;
            Debug.Log($"OnKillFrenzyCharge modifier applied to {fireball.Name}");
        }

        public void Remove(IModifiable target) {
            if (target is FireballSkill fireball)
                fireball.OnKill -= HandleKill;
        }

        private void HandleKill(KillContext context) {
            _frenzyCharges++;
            Debug.Log($"Gained Frenzy Charge! Total: {_frenzyCharges}");
        }

        private bool HasMatchingTags(IModifiable target) {
            if (RequiredTags.Count == 0)
                return true;

            foreach (var tag in RequiredTags) {
                if (target.Tags.Contains(tag))
                    return true;
            }

            return false;
        }
    }
}