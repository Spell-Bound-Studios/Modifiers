// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// The core engine of the library. Holds base stat values + modifier list for one target (character, item,
    /// tree, chest — anything) and computes final values in PoE order:
    /// <c>Base -> Flat -> Increased (additive pool) -> More (multiplicative chain) -> Override (last wins)</c>.
    /// Stats are addressed by integer id (interned through <see cref="StatRegistry"/>); user code addresses
    /// them by name via the extension methods in <see cref="BehaviourExtensions"/>.
    /// </summary>
    /// <remarks>
    /// Values are stored as fixed-point ints (scale = <see cref="StatSettings.Precision"/>) so the math is
    /// deterministic across machines and survives serialization round-trips. Recalculation is dirty-flagged:
    /// <see cref="GetValue"/> only re-runs <see cref="CalculateStat"/> when a modifier was added/removed since
    /// the last read. <see cref="IPacker"/> is implemented so a container can ride inside any packed data slot
    /// (chunk data, save file, network frame); the wire format keys by stat NAME, not registry id, because
    /// registry ids are process-local.
    /// </remarks>
    /// <summary>
    /// Base class for a pure capability. A behaviour knows HOW to do exactly one thing (fire a projectile,
    /// receive damage, emit a beam, hold a resource pool, run a duration effect) and owns its own
    /// <see cref="StatContainer"/> for the numbers that govern that thing. It does NOT know when it runs,
    /// what triggers it, or what comes after — that orchestration is the GAME's job (see the README).
    /// </summary>
    /// <remarks>
    /// Subclass and override <see cref="InitializeStats"/> to seed base values; everything else (cooldowns,
    /// triggers, FX, networking) is layered on by the consuming game. The <see cref="SerializableAttribute"/>
    /// is required so concrete subclasses can ride a <c>[SerializeReference]</c> field for designer authoring.
    /// </remarks>
    [Serializable]
    public class SbBehaviour : ISerializationCallbackReceiver {
        // Base values before any modifiers are applied (stored as fixed-point ints)
        private readonly Dictionary<int, int> _baseValues = new();

        // Cached calculated values (stored as fixed-point ints, only valid when !_isDirty)
        private readonly Dictionary<int, int> _calculatedValues = new();

        // All active modifiers affecting this entity
        private readonly Dictionary<int, List<StatModifierEntry>> _modifiersByStatId = new();

        // If true, we need to recalculate before returning values
        private bool _isDirty = true;

        #region Inspector Authoring

        [SerializeField] private List<StatBaseEntry> stats = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            if (stats == null)
                return;

            foreach (var entry in stats) {
                if (entry.stat == null || string.IsNullOrEmpty(entry.stat.StatName))
                    continue;

                SetBase(entry.stat.StatName, entry.baseValue);
            }
        }

        #endregion

        /// <summary>
        /// Set the base value for a stat before modifiers.
        /// </summary>
        /// <example>
        /// Base physical damage = 100
        /// </example>
        public void SetBase(int statId, float value) {
            _baseValues[statId] = StatSettings.ToInternal(value);
            NotifyDirty();
        }

        /// <summary>
        /// Fires whenever stats may have changed (base set, modifier added/removed, etc.). UI / debug
        /// surfaces subscribe and re-read values; the engine itself only flips the dirty flag and
        /// recomputes lazily on next <see cref="GetValue"/>.
        /// </summary>
        public event Action OnStatsDirty;

        protected virtual void NotifyDirty() {
            _isDirty = true;
            OnStatsDirty?.Invoke();
        }

        /// <summary>
        /// Get the base value for a stat before modifiers.
        /// Returns 0 if the stat hasn't been set.
        /// </summary>
        public float GetBase(int statId) =>
                _baseValues.TryGetValue(statId, out var value)
                        ? StatSettings.ToExternal(value)
                        : 0f;

        public bool HasBase(int statId) => _baseValues.ContainsKey(statId);

        /// <summary>
        /// Add a modifier to this container.
        /// The modifier will be applied during the next calculation.
        /// </summary>
        public void AddModifier(StatModifierEntry modifierEntry) {
            if (!_modifiersByStatId.ContainsKey(modifierEntry.StatId))
                _modifiersByStatId[modifierEntry.StatId] = new List<StatModifierEntry>();

            _modifiersByStatId[modifierEntry.StatId].Add(modifierEntry);
            NotifyDirty();
        }

        /// <summary>
        /// Remove all modifiers from a specific id.
        /// Use this when unequipping an item, removing a buff, etc.
        /// </summary>
        public void RemoveModifierByUniqueId(string uniqueId) {
            if (string.IsNullOrEmpty(uniqueId)) {
                Log.Error("Attempting to remove a modifier with a null ID.");

                return;
            }

            foreach (var modifierList in _modifiersByStatId.Values)
                modifierList.RemoveAll(m => m.UniqueId == uniqueId);

            NotifyDirty();
        }

        /// <summary>
        /// Get the final calculated value for a stat (base + all modifiers applied).
        /// Triggers recalculation if needed.
        /// </summary>
        public float GetValue(int statId) {
            if (_isDirty)
                Recalculate();

            if (_calculatedValues.TryGetValue(statId, out var value))
                return StatSettings.ToExternal(value);

            return GetBase(statId);
        }

        public void ClearModifiers() {
            _modifiersByStatId.Clear();
            NotifyDirty();
        }

        public void Clear() {
            _baseValues.Clear();
            _modifiersByStatId.Clear();
            _calculatedValues.Clear();
            NotifyDirty();
        }

        public int StatCount => _baseValues.Count;

        public int ModifierCount => _modifiersByStatId.Values.Sum(list => list.Count);

        /// <summary>
        /// Read-only enumeration of every stat id with a base value set. Used by UI / debug surfaces to walk
        /// the live stat list without bypassing the engine's dictionaries.
        /// </summary>
        public IEnumerable<int> StatIds => _baseValues.Keys;

        #region Name-Based Overloads

        /// <summary>Name-keyed <see cref="SetBase(int, float)"/>; interns the name via <see cref="StatRegistry"/>.</summary>
        public void SetBase(string statName, float value) => SetBase(StatRegistry.Register(statName), value);

        /// <summary>Name-keyed <see cref="GetBase(int)"/>; interns the name via <see cref="StatRegistry"/>.</summary>
        public float GetBase(string statName) => GetBase(StatRegistry.Register(statName));

        /// <summary>Name-keyed <see cref="HasBase(int)"/>; interns the name via <see cref="StatRegistry"/>.</summary>
        public bool HasBase(string statName) => HasBase(StatRegistry.Register(statName));

        /// <summary>Name-keyed <see cref="GetValue(int)"/>; interns the name via <see cref="StatRegistry"/>.</summary>
        public float GetValue(string statName) => GetValue(StatRegistry.Register(statName));

        /// <summary>
        /// Add a <see cref="ModifierType.Flat"/> modifier to the named stat. The optional
        /// <paramref name="uniqueId"/> lets the caller later remove this exact modifier via
        /// <see cref="RemoveModifierByUniqueId"/>.
        /// </summary>
        public void AddFlat(string statName, float value, string uniqueId = null) =>
                AddModifier(new StatModifierEntry(
                    StatRegistry.Register(statName),
                    ModifierType.Flat,
                    value,
                    uniqueId));

        /// <summary>
        /// Add a <see cref="ModifierType.Increased"/> modifier (additive % pool — stacks of Increased sum
        /// together before the multiplication) to the named stat.
        /// </summary>
        public void AddIncreased(string statName, float percent, string uniqueId = null) =>
                AddModifier(new StatModifierEntry(
                    StatRegistry.Register(statName),
                    ModifierType.Increased,
                    percent,
                    uniqueId));

        /// <summary>
        /// Add a <see cref="ModifierType.More"/> modifier (multiplicative — each More multiplies the running
        /// total) to the named stat.
        /// </summary>
        public void AddMore(string statName, float percent, string uniqueId = null) =>
                AddModifier(new StatModifierEntry(
                    StatRegistry.Register(statName),
                    ModifierType.More,
                    percent,
                    uniqueId));

        /// <summary>
        /// Add a <see cref="ModifierType.Override"/> modifier (last-Override-wins; ignores Base / Flat /
        /// Increased / More entirely) to the named stat. Useful for absolute setters — debug console
        /// "SetSpeed 10", talents that force a stat to a fixed value, etc.
        /// </summary>
        public void AddOverride(string statName, float value, string uniqueId = null) =>
                AddModifier(new StatModifierEntry(
                    StatRegistry.Register(statName),
                    ModifierType.Override,
                    value,
                    uniqueId));

        #endregion

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
        /// Uses fixed-point integer math for deterministic calculations.
        /// </summary>
        private int CalculateStat(int statId, List<StatModifierEntry> modifiers) {
            var baseValue = _baseValues.GetValueOrDefault(statId, 0);
            var precision = StatSettings.Precision;

            // Step 1: Apply flat modifiers
            var flatSum = 0;

            foreach (var mod in modifiers) {
                if (mod.Type == ModifierType.Flat)
                    flatSum += StatSettings.ToInternal(mod.Value);
            }

            var afterFlat = baseValue + flatSum;

            // Step 2: Apply all Increased modifiers - they stack additively
            // Example: 30% + 20% + 50% = 100% increased = multiply by 2.0
            var increasedSum = 0;

            foreach (var mod in modifiers) {
                if (mod.Type == ModifierType.Increased)
                    increasedSum += StatSettings.ToInternal(mod.Value);
            }

            // (base + flat) * (1 + increased/100)
            // In fixed-point: afterFlat * (precision + increasedSum) / precision
            var afterIncreased = (long)afterFlat * (precision + increasedSum) / precision;

            // Step 3: Apply all More modifiers - each is multiplicative
            // Example: 40% more and then 30% more = 1.4 * 1.3 = 1.82 (82% total increase)
            var afterMore = afterIncreased;

            foreach (var mod in modifiers) {
                if (mod.Type == ModifierType.More) {
                    var moreValue = StatSettings.ToInternal(mod.Value);
                    afterMore = afterMore * (precision + moreValue) / precision;
                }
            }

            // Step 4: Check for Override modifiers (last one wins, ignores all previous calculations)
            foreach (var mod in modifiers) {
                if (mod.Type == ModifierType.Override)
                    return StatSettings.ToInternal(mod.Value);
            }

            return (int)afterMore;
        }

        #region IPacker

        /// <summary>
        /// Pack the container as base values + flattened modifier list, keyed by stat NAME (not the process-local
        /// integer id from <see cref="StatRegistry"/>). Names survive load-order changes, database reordering, and
        /// cross-process transfer.
        /// </summary>
        public void Pack(ref Span<byte> buffer) {
            Packer.WriteInt(ref buffer, _baseValues.Count);

            foreach (var (id, value) in _baseValues) {
                Packer.WriteString(ref buffer, StatRegistry.GetName(id));
                Packer.WriteInt(ref buffer, value);
            }

            Packer.WriteInt(ref buffer, ModifierCount);

            foreach (var (id, modifiers) in _modifiersByStatId) {
                var name = StatRegistry.GetName(id);

                foreach (var mod in modifiers) {
                    Packer.WriteString(ref buffer, name);
                    Packer.WriteByte(ref buffer, (byte)mod.Type);
                    Packer.WriteFloat(ref buffer, mod.Value);
                    Packer.WriteString(ref buffer, mod.UniqueId ?? string.Empty);
                }
            }
        }

        /// <summary>
        /// Replace the container's contents from the buffer. Re-registers any stat names encountered through
        /// <see cref="StatRegistry"/>, which means strict validation must already be configured if you want unknown
        /// stats to throw on unpack.
        /// </summary>
        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            Clear();

            var baseCount = Packer.ReadInt(ref buffer);

            for (var i = 0; i < baseCount; i++) {
                var name = Packer.ReadString(ref buffer);
                var value = Packer.ReadInt(ref buffer);
                var id = StatRegistry.Register(name);
                _baseValues[id] = value;
            }

            var modCount = Packer.ReadInt(ref buffer);

            for (var i = 0; i < modCount; i++) {
                var name = Packer.ReadString(ref buffer);
                var type = (ModifierType)Packer.ReadByte(ref buffer);
                var value = Packer.ReadFloat(ref buffer);
                var uniqueId = Packer.ReadString(ref buffer);
                var id = StatRegistry.Register(name);

                if (!_modifiersByStatId.TryGetValue(id, out var list)) {
                    list = new List<StatModifierEntry>();
                    _modifiersByStatId[id] = list;
                }

                list.Add(new StatModifierEntry(id, type, value, uniqueId));
            }

            _isDirty = true;
        }

        #endregion

        #region Display Helpers

        /// <summary>
        /// Get a formatted string of all base stats for debug output.
        /// </summary>
        public string GetBaseStatList() {
            var lines = _baseValues
                    .Select(kvp => {
                        var name = StatRegistry.GetName(kvp.Key) ?? $"Unknown({kvp.Key})";
                        var value = StatSettings.ToExternal(kvp.Value);

                        return $"  {name}: {value}";
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

            var flats = modifiers
                    .Where(m => m.Type == ModifierType.Flat)
                    .Select(m => m.Value)
                    .ToList();

            var increases = modifiers
                    .Where(m => m.Type == ModifierType.Increased)
                    .Select(m => m.Value)
                    .ToList();

            var mores = modifiers
                    .Where(m => m.Type == ModifierType.More)
                    .Select(m => m.Value)
                    .ToList();

            var overrides = modifiers
                    .Where(m => m.Type == ModifierType.Override)
                    .Select(m => m.Value)
                    .ToList();

            if (flats.Count > 0)
                lines.Add($"Flat: {string.Join(", ", flats.Select(f => $"+{f}"))} (Total: +{flats.Sum()})");

            if (increases.Count > 0) {
                lines.Add(
                    $"Increased: {string.Join(", ", increases.Select(i => $"{i}%"))} (Total: {increases.Sum()}%)");
            }

            if (mores.Count > 0) {
                var moreProduct = mores.Aggregate(1f, (acc, m) => acc * (1f + m / 100f));

                lines.Add(
                    $"More: {string.Join(", ", mores.Select(m => $"{m}%"))} (Total: {(moreProduct - 1f) * 100f:F2}%)");
            }

            if (overrides.Count > 0)
                lines.Add($"Override: {overrides.Last()} (ignores all calculations)");

            lines.Add($"Final: {GetValue(statId):F2}");

            return string.Join("\n", lines);
        }

        #endregion
    }
}