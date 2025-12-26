using System.Collections.Generic;

namespace Spellbound.Stats
{
    /// <summary>
    /// Holds base stat values and modifiers for an entity (character, item, etc.).
    /// Calculates final stat values by applying modifiers in the correct order.
    /// Uses dirty flagging to avoid unnecessary recalculation.
    /// </summary>
    public class StatContainer
    {
        // Base values before any modifiers are applied
        private readonly Dictionary<int, float> _baseValues = new();
        
        // All active modifiers affecting this entity
        private readonly List<StatModifier> _modifiers = new();
        
        // Cached calculated values (only valid when !_isDirty)
        private readonly Dictionary<int, float> _calculatedValues = new();
        
        // If true, we need to recalculate before returning values
        private bool _isDirty = true;

        /// <summary>
        /// Set the base value for a stat (before modifiers).
        /// Example: Base physical damage = 100
        /// </summary>
        public void SetBase(int statId, float value)
        {
            _baseValues[statId] = value;
            _isDirty = true;
        }

        /// <summary>
        /// Get the base value for a stat (before modifiers).
        /// Returns 0 if the stat hasn't been set.
        /// </summary>
        public float GetBase(int statId)
        {
            return _baseValues.TryGetValue(statId, out float value) ? value : 0f;
        }

        /// <summary>
        /// Add a modifier to this container.
        /// The modifier will be applied during the next calculation.
        /// </summary>
        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            _isDirty = true;
        }

        /// <summary>
        /// Remove all modifiers from a specific source.
        /// Use this when unequipping an item, removing a buff, etc.
        /// </summary>
        public void RemoveModifiersFromSource(int sourceId)
        {
            _modifiers.RemoveAll(m => m.SourceId == sourceId);
            _isDirty = true;
        }

        /// <summary>
        /// Get the final calculated value for a stat (base + all modifiers applied).
        /// Triggers recalculation if needed.
        /// </summary>
        public float GetValue(int statId)
        {
            if (_isDirty)
                Recalculate();

            return _calculatedValues.TryGetValue(statId, out float value) ? value : GetBase(statId);
        }

        /// <summary>
        /// Recalculate all stats by applying modifiers in the correct order.
        /// TODO: This is where the magic happens - we'll implement the calculation logic next.
        /// </summary>
        private void Recalculate()
        {
            _calculatedValues.Clear();

            // TODO: Group modifiers by stat
            // TODO: Apply in order: Flat -> Increased -> More -> Override
            // TODO: Handle conditional modifiers (TagMask)

            _isDirty = false;
        }
    }
}