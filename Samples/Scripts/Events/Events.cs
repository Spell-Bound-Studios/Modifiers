// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Fired when a skill begins casting.
    /// </summary>
    public readonly struct CastEvent : ISkillEvent {
        public readonly Skill Skill;
        public readonly Vector3 Origin;
        public readonly Vector3 Direction;

        public CastEvent(Skill skill, Vector3 origin, Vector3 direction) {
            Skill = skill;
            Origin = origin;
            Direction = direction;
        }
    }

    /// <summary>
    /// Fired when a projectile hits something.
    /// </summary>
    public readonly struct HitEvent : ISkillEvent {
        public readonly Skill Skill;
        public readonly IBehaviour SourceBehaviour;
        public readonly GameObject Target;
        public readonly Vector3 Position;

        public HitEvent(Skill skill, IBehaviour source, GameObject target, Vector3 position) {
            Skill = skill;
            SourceBehaviour = source;
            Target = target;
            Position = position;
        }
    }

    /// <summary>
    /// Fired when something dies from skill damage.
    /// </summary>
    public readonly struct KillEvent : ISkillEvent {
        public readonly Skill Skill;
        public readonly ICanBeModified Victim;

        public KillEvent(Skill skill, ICanBeModified victim) {
            Skill = skill;
            Victim = victim;
        }
    }
}