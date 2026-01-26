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
        /// Set the base value for a stat before modifiers.
        /// </summary>
        /// <example>
        /// Base physical damage = 100
        /// </example>
        public void SetBase(int statId, float value) {
            _baseValues[statId] = value;
            _isDirty = true;
        }

        /// <summary>
        /// Get the base value for a stat before modifiers.
        /// Returns 0 if the stat hasn't been set.
        /// </summary>
        public float GetBase(int statId) => _baseValues.GetValueOrDefault(statId, 0f);
        public bool HasBase(int statId) => _baseValues.ContainsKey(statId);

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
        
        public void ClearModifiers() {
            _modifiersByStatId.Clear();
            _isDirty = true;
        }

        public void Clear() {
            _baseValues.Clear();
            _modifiersByStatId.Clear();
            _calculatedValues.Clear();
            _isDirty = true;
        }

        public int StatCount => _baseValues.Count;

        public int ModifierCount => _modifiersByStatId.Values.Sum(list => list.Count);

        /// <summary>
        /// Recalculate all stats by applying modifiers in the correct order.
        /// Order: Base -> Flat additions -> Increased (additive pool) -> More (multiplicative chain) -> Override
        /// </summary>
        private void Recalculate() {
            _calculatedValues.Clear();
            
            foreach (var (statId, modifiers) in _modifiersByStatId)
                _calculatedValues[statId] = CalculateStat(statId, modifiers);

            _isDirty = false;
        }

        /// <summary>
        /// Calculate a single stat's final value by applying modifiers in PoE order.
        /// </summary>
        private float CalculateStat(int statId, List<StatModifier> modifiers) {
            var baseValue = GetBase(statId);

            // Step 1: Apply flat mod
            var flatSum = 0f;
            foreach (var mod in modifiers)
                if (mod.Type == ModifierType.Flat)
                    flatSum += mod.Value;

            var afterFlat = baseValue + flatSum;

            // Step 2: Apply all Increased modifiers - they stack additively
            // Example: 30% + 20% + 50% = 100% increased = multiply by 2.0
            var increasedSum = 0f;
            foreach (var mod in modifiers)
                if (mod.Type == ModifierType.Increased)
                    increasedSum += mod.Value;

            var afterIncreased = afterFlat * (1f + increasedSum / 100f);

            // Step 3: Apply all More modifiers - each is multiplicative
            // Example: 40% more and then 30% more = 1.4 * 1.3 = 1.82 (82% total increase)
            var afterMore = afterIncreased;
            foreach (var mod in modifiers)
                if (mod.Type == ModifierType.More)
                    afterMore *= 1f + mod.Value / 100f;

            // Step 4: Check for Override modifiers (last one wins, ignores all previous calculations)
            foreach (var mod in modifiers)
                if (mod.Type == ModifierType.Override)
                    return mod.Value;

            return afterMore;
        }

        #region Display Helpers

        /// <summary>
        /// Get a formatted string of all base stats for debug output.
        /// </summary>
        public string GetBaseStatList() {
            var lines = _baseValues
                .Select(kvp => {
                    var name = StatRegistry.GetName(kvp.Key) ?? $"Unknown({kvp.Key})";
                    return $"  {name}: {kvp.Value}";
                });

            return string.Join("\n", lines);
        }
        
        /// <summary>
        /// Get a formatted string of all calculated stats for debug output.
        /// Triggers recalculation if needed.
        /// </summary>
        public string GetCalculatedStatList() {
            if (_isDirty) 
                Recalculate();

            var allStatIds = new HashSet<int>(_baseValues.Keys);
            foreach (var statId in _modifiersByStatId.Keys)
                allStatIds.Add(statId);

            var lines = allStatIds
                .Select(statId => {
                    var name = StatRegistry.GetName(statId) ?? $"Unknown({statId})";
                    var baseValue = GetBase(statId);
                    var finalValue = GetValue(statId);
                    return $"  {name}: {finalValue:F2} (base: {baseValue})";
                });

            return string.Join("\n", lines);
        }
        
        /// <summary>
        /// Get a detailed breakdown of how a stat's value is calculated.
        /// Shows all modifiers grouped by type and the step-by-step calculation.
        /// </summary>
        public string GetModifierAnalysis(int statId) {
            var statName = StatRegistry.GetName(statId) ?? $"Unknown({statId})";
            var baseValue = GetBase(statId);
            var lines = new List<string> {
                $"Stat: {statName}",
                $"Base: {baseValue}"
            };

            if (!_modifiersByStatId.TryGetValue(statId, out var modifiers) || modifiers.Count == 0) {
                lines.Add("No modifiers");
                lines.Add($"Final: {baseValue}");
                return string.Join("\n", lines);
            }

            var flats = modifiers.Where(m => m.Type == ModifierType.Flat).Select(m => m.Value).ToList();
            var increases = modifiers.Where(m => m.Type == ModifierType.Increased).Select(m => m.Value).ToList();
            var mores = modifiers.Where(m => m.Type == ModifierType.More).Select(m => m.Value).ToList();
            var overrides = modifiers.Where(m => m.Type == ModifierType.Override).Select(m => m.Value).ToList();

            if (flats.Count > 0)
                lines.Add($"Flat: {string.Join(", ", flats.Select(f => $"+{f}"))} (Total: +{flats.Sum()})");

            if (increases.Count > 0)
                lines.Add($"Increased: {string.Join(", ", increases.Select(i => $"{i}%"))} (Total: {increases.Sum()}%)");

            if (mores.Count > 0) {
                var moreProduct = mores.Aggregate(1f, (acc, m) => acc * (1f + m / 100f));
                lines.Add($"More: {string.Join(", ", mores.Select(m => $"{m}%"))} (Total: {(moreProduct - 1f) * 100f:F2}%)");
            }

            if (overrides.Count > 0)
                lines.Add($"Override: {overrides.Last()} (ignores all calculations)");

            lines.Add($"Final: {GetValue(statId):F2}");

            return string.Join("\n", lines);
        }

        #endregion
    }
}