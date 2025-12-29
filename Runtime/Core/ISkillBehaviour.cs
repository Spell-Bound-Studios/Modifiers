// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Stats {
    // Behaviors are components that provide capabilities
    public interface ISkillBehaviour {
        string BehaviourType { get; } // "Projectile", "AoE", "Fire", etc.
        void Execute();
    }
}