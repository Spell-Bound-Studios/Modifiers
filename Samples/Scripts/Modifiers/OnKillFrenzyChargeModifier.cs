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

        private int _frenzyChargesGranted = 1;

        public OnKillFrenzyChargeModifier(int modifierId, HashSet<int> requiredTags) {
            ModifierId = modifierId;
            RequiredTags = requiredTags;
        }

        public void Apply(IModifiable target) {
            if (!HasMatchingTags(target))
                return;

            if (target is not Skill skill) {
                Debug.LogError("Attempting to modify something that is not a skill but requires it to be.");
                return;
            }
            
            var projectile = skill.GetBehaviour<ProjectileBehaviour>();
            
            if (projectile == null) {
                projectile = new ProjectileBehaviour();
                skill.AddBehaviour(projectile);
            }
            
            projectile.OnKill += HandleKill;
            Debug.Log($"OnKillFrenzyCharge modifier applied to {skill.Name}");
        }

        public void Remove(IModifiable target) {
            if (target is not Skill skill || !skill.HasBehaviour<ProjectileBehaviour>()) {
                Debug.LogError("Attempting to remove a behaviour from something that does not contain that behaviour.");
                return;
            }

            var projectile = skill.GetBehaviour<ProjectileBehaviour>();
            projectile.OnKill -= HandleKill;
        }

        private void HandleKill(object context) {
            Debug.Log($"Gained Frenzy Charge! Total: {_frenzyChargesGranted}");
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