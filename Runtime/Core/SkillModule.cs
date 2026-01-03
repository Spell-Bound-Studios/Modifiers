// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core;
using Spellbound.Stats.Attributes;
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
        
        [Tooltip("Base tags (Fire, Cold, Spell, Attack, Projectile, etc.)")]
        [SerializeField] private List<TagEntry> baseTags = new();
        
        [Tooltip("Base stat values (damage, cast_speed, etc.)")]
        [SerializeField] private List<BaseStatValue> baseStats = new();
        
        [Serializable]
        public struct TagEntry {
            [TagId] public int tagId;
        }
        
        [Serializable]
        public struct BaseStatValue {
            [StatId] public int statId;
            public float value;
        }
        
        public override SbbData? GetData(ObjectPreset preset) {
            return null;
        }
        
        public Skill CreateSkill() {
            var skill = new Skill(skillName);
            
            foreach (var tagEntry in baseTags)
                skill.Tags.Add(tagEntry.tagId);
            
            foreach (var baseStat in baseStats)
                skill.Stats.SetBase(baseStat.statId, baseStat.value);
            
            return skill;
        }
    }
}