// Copyright 2025 Spellbound Studio Inc.

#nullable enable
using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Designer interface / API.
    /// Provides common functionality to avoid code duplication.
    /// </summary>
    public static class SbModifier {
        /// <summary>
        /// Check if a modifier should apply to a target based on tag matching.
        /// </summary>
        /// <param name="requiredTags">Tags required by the modifier (null = unconditional)</param>
        /// <param name="target">Target to check against</param>
        /// <returns>True if modifier should apply, false otherwise</returns>
        /// <example>
        /// if (!ModifierUtility.ShouldApply(RequiredTags, target)) return;
        /// </example>
        public static bool ShouldModifierApply(HashSet<int>? requiredTags, IModifiable target) {
            // Null or empty = applies to everything
            if (requiredTags == null || requiredTags.Count == 0)
                return true;

            // Check for any tag overlap
            foreach (var tag in requiredTags) {
                if (target.Tags.Contains(tag))
                    return true;
            }

            return false;
        }
    }
}