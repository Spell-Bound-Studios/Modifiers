// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;

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
        /// 
        /// </summary>
        /// <example>
        /// 
        /// </example>
        string BehaviourType { get; }
        
        /// <summary>
        /// An ID that allows the user to associate stats that are impacted by a modifier with matching tags.
        /// </summary>
        /// <example>
        /// A "Fire" tag impacts the fire_damage and chance_to_burn stats.
        /// </example>
        HashSet<int> Tags { get; }
        
        /// <summary>
        /// Stats specific to this behaviour.
        /// Modifiers target these stats when matching tags.
        /// </summary>
        StatContainer Stats { get; }
    }
}