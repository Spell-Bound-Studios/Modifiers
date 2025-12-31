// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Stats {
    /// <summary>
    /// Implement this if you want to create a behaviour that can be attached to something.
    /// Behaviours are composable and define what something can do.
    /// Examples: 
    /// </summary>
    /// <example>
    /// ProjectileBehaviour (fires projectiles), FireBehaviour (burns something), etc.
    /// </example>
    public interface IBehaviour {
        /// <summary>
        /// Unique identifier for this behaviour type.
        /// Used to retrieve specific behaviours from an implementer.
        /// </summary>
        /// <example>
        /// "Projectile", "AoE", "Fire", etc.
        /// </example>
        string BehaviourType { get; }
    }
}