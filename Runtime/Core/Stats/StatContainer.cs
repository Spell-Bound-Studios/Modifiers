using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spellbound.Stats {
    /// <summary>
    /// Holds base stat values and modifiers for an entity (character, item, etc.).
    /// Calculates final stat values by applying modifiers in the correct order.
    /// Uses dirty flagging to avoid unnecessary recalculation.
    /// </summary>
    public class StatContainer {
        // Base values before any modifiers are applied
        private readonly Dictionary<int, float> _baseValues = new();

        // Cached calculated values (only valid when !_isDirty)
        private readonly Dictionary<int, float> _calculatedValues = new();

        // All active modifiers affecting this entity
        private readonly Dictionary<int, List<StatModifier>> _modifiersByStatId = new();

        // If true, we need to recalculate before returning values
        private bool _isDirty = true;

        /// <summary>
        /// Set the base value for a stat (before modifiers).
        /// </summary>
        /// <example>
        /// Base physical damage = 100
        /// </example>
        public void SetBase(int statId, float value) {
            _baseValues[statId] = value;
            _isDirty = true;
        }

        /// <summary>
        /// Get the base value for a stat (before modifiers).
        /// Returns 0 if the stat hasn't been set.
        /// </summary>
        public float GetBase(int statId) => _baseValues.GetValueOrDefault(statId, 0f);

        /// <summary>
        /// Add a modifier to this container.
        /// The modifier will be applied during the next calculation.
        /// </summary>
        public void AddModifier(StatModifier modifier) {
            if (!_modifiersByStatId.ContainsKey(modifier.StatId))
                _modifiersByStatId[modifier.StatId] = new List<StatModifier>();

            _modifiersByStatId[modifier.StatId].Add(modifier);
            _isDirty = true;
        }

        /// <summary>
        /// Remove all modifiers from a specific source.
        /// Use this when unequipping an item, removing a buff, etc.
        /// </summary>
        public void RemoveModifiersFromSource(int sourceId) {
            foreach (var modifierList in _modifiersByStatId.Values)
                modifierList.RemoveAll(m => m.SourceId == sourceId);

            _isDirty = true;
        }

        /// <summary>
        /// Get the final calculated value for a stat (base + all modifiers applied).
        /// Triggers recalculation if needed.
        /// </summary>
        public float GetValue(int statId) {
            if (_isDirty)
                Recalculate();

            return _calculatedValues.TryGetValue(statId, out var value) ? value : GetBase(statId);
        }

        /// <summary>
        /// Recalculate all stats by applying modifiers in the correct order.
        /// Order: Base -> Flat additions -> Increased (additive pool) -> More (multiplicative chain) -> Override
        /// </summary>
        private void Recalculate() {
            _calculatedValues.Clear();
            
            foreach (var (statId, modifiers) in _modifiersByStatId) {
                var finalValue = CalculateStat(statId, modifiers);
                _calculatedValues[statId] = finalValue;
            }

            _isDirty = false;
        }

        /// <summary>
        /// Calculate a single stat's final value by applying modifiers in PoE order.
        /// </summary>
        private float CalculateStat(int statId, List<StatModifier> modifiers) {
            var baseValue = GetBase(statId);

            // Step 1: Apply flat mod
            var flatSum = 0f;

            foreach (var mod in modifiers) {
                if (mod.Type == ModifierType.Flat)
                    flatSum += mod.Value;
            }

            var afterFlat = baseValue + flatSum;

            // Step 2: Apply all Increased modifiers - they stack additively
            // Example: 30% + 20% + 50% = 100% increased = multiply by 2.0
            var increasedSum = 0f;

            foreach (var mod in modifiers) {
                if (mod.Type == ModifierType.Increased)
                    increasedSum += mod.Value;
            }

            var afterIncreased = afterFlat * (1f + increasedSum / 100f);

            // Step 3: Apply all More modifiers - each is multiplicative
            // Example: 40% more and then 30% more = 1.4 * 1.3 = 1.82 (82% total increase)
            var afterMore = afterIncreased;

            foreach (var mod in modifiers) {
                if (mod.Type == ModifierType.More)
                    afterMore *= 1f + mod.Value / 100f;
            }

            // Step 4: Check for Override modifiers (last one wins, ignores all previous calculations)
            foreach (var mod in modifiers) {
                if (mod.Type == ModifierType.Override)
                    return mod.Value;
            }

            return afterMore;
        }

        #region Display Helpers

        /// <summary>
        /// Get a formatted string of all base stats for debug output.
        /// </summary>
        public string GetBaseStatList() {
            var lines = new List<string>();

            foreach (var kvp in _baseValues) {
                var statName = StatRegistry.GetName(kvp.Key);

                if (string.IsNullOrEmpty(statName)) {
                    Debug.LogError($"Stat {kvp.Key} has no stat name");

                    continue;
                }

                lines.Add($"  {statName}: {kvp.Value}");
            }

            return string.Join("\n", lines);
        }
        
        /// <summary>
        /// Get a formatted string of all calculated stats for debug output.
        /// Triggers recalculation if needed.
        /// </summary>
        public string GetCalculatedStatList() {
            if (_isDirty)
                Recalculate();
    
            var lines = new List<string>();

            // Show all stats that have either a base value or modifiers
            var allStatIds = new HashSet<int>(_baseValues.Keys);
            foreach (var statId in _modifiersByStatId.Keys)
                allStatIds.Add(statId);

            foreach (var statId in allStatIds) {
                var statName = StatRegistry.GetName(statId);

                if (string.IsNullOrEmpty(statName)) {
                    Debug.LogError($"Stat {statId} has no stat name");
                    continue;
                }

                var baseValue = GetBase(statId);
                var finalValue = GetValue(statId);
                lines.Add($"  {statName}: {finalValue:F2} (base: {baseValue})");
            }

            return string.Join("\n", lines);
        }
        
        /// <summary>
        /// Get a detailed breakdown of how a stat's value is calculated.
        /// Shows all modifiers grouped by type and the step-by-step calculation.
        /// </summary>
        public string GetModifierAnalysis(int statId) {
            var statName = StatRegistry.GetName(statId);
            var baseValue = GetBase(statId);
            var lines = new List<string> {
                $"Stat Name: {statName}",
                $"Base: {baseValue}"
            };

            if (!_modifiersByStatId.TryGetValue(statId, out var modifiers) || modifiers.Count == 0) {
                lines.Add("No modifiers");
                lines.Add($"Final: {baseValue}");
                return string.Join("\n", lines);
            }
            
            // Collect modifiers by type
            var flats = new List<float>();
            var increases = new List<float>();
            var mores = new List<float>();
            var overrides = new List<float>();
            
            foreach (var mod in modifiers) {
                switch (mod.Type) {
                    case ModifierType.Flat:
                        flats.Add(mod.Value);
                        break;
                    case ModifierType.Increased:
                        increases.Add(mod.Value);
                        break;
                    case ModifierType.More:
                        mores.Add(mod.Value);
                        break;
                    case ModifierType.Override:
                        overrides.Add(mod.Value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            // flat modifiers
            if (flats.Count > 0) {
                var flatSum = flats.Sum();
                lines.Add($"Flat: {string.Join(", ", flats.Select(f => $"+{f}"))} (Total: +{flatSum})");
            }
            
            // increased modifiers
            if (increases.Count > 0) {
                var increasedSum = increases.Sum();
                lines.Add($"Increased: {string.Join(", ", increases.Select(i => $"{i}%"))} (Total: {increasedSum}%)");
            }
            
            // more modifiers
            if (mores.Count > 0) {
                var moreProduct = mores.Aggregate(1f, (acc, m) => acc * (1f + m / 100f));
                var totalMorePercent = (moreProduct - 1f) * 100f;
                lines.Add($"More: {string.Join(", ", mores.Select(m => $"{m}%"))} (Total: {totalMorePercent:F2}%)");
            }
            
            // overrides if any
            if (overrides.Count > 0)
                lines.Add($"Override: {overrides.Last()} (ignores all calculations)");
            
            lines.Add($"Final: {GetValue(statId):F2}");
            
            return string.Join("\n", lines);
        }

        #endregion
    }
}