// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core;
using UnityEngine;

namespace Spellbound.Stats {
    /// <summary>
    /// Defines a skill's base properties for a Skill ObjectPreset.
    /// Creates Skill instances at runtime with configured stats, tags, and behaviours.
    /// </summary>
    [Serializable]
    public class SkillModule : PresetModule {
        [Tooltip("Display name for this skill")]
        [SerializeField] private string skillName;
        
        [Tooltip("Behaviours this skill has (Fire, Projectile, Duration, etc.)")]
        [SerializeReference] private List<IBehaviour> behaviours = new();

        public override SbbData? GetData(ObjectPreset preset) => null;
        
        public Skill CreateSkill() {
            var skill = new Skill(skillName);
            
            foreach (var behaviour in behaviours)
                skill.AddBehaviour(behaviour);
            
            return skill;
        }
    }
}