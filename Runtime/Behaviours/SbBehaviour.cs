// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Base class for a pure capability. A behaviour knows HOW to do exactly one thing (fire a projectile,
    /// receive damage, emit a beam, hold a resource pool, run a duration effect) and owns the stats that
    /// govern that thing, computed in PoE order:
    /// <c>Base -> Flat -> Increased (additive pool) -> More (multiplicative chain) -> Override (first wins)</c>.
    /// Stats are keyed by the stable hash of their name (via <see cref="StatRegistry"/>).
    /// </summary>
    /// <remarks>
    /// Values are stored as fixed-point ints (scale = <see cref="StatSettings.Precision"/>) so the math is
    /// deterministic across machines and survives serialization round-trips. Recalculation is dirty-flagged.
    /// <see cref="IPacker"/> keys the wire format by stat hash — a stable 4-byte id identical on every machine.
    /// The <see cref="SerializableAttribute"/> is required so concrete subclasses can ride a
    /// <c>[SerializeReference]</c> field for designer authoring.
    /// </remarks>
    [Serializable]
    public class SbBehaviour : ISerializationCallbackReceiver {
        // Base values before any modifiers are applied (stored as fixed-point ints)
        private readonly Dictionary<uint, int> _baseValues = new();

        // Cached calculated values (stored as fixed-point ints, only valid when !_isDirty)
        private readonly Dictionary<uint, int> _calculatedValues = new();

        // All active modifiers affecting this entity
        private readonly Dictionary<uint, List<StatModifierEntry>> _modifiersByStatId = new();

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

                SetBase(entry.stat.Hash, entry.baseValue);
            }
        }

        #endregion

        /// <summary>
        /// Set the base value for a stat before modifiers.
        /// </summary>
        public void SetBase(uint statHash, float value) {
            _baseValues[statHash] = StatSettings.ToInternal(value);
            NotifyDirty();
        }

        /// <summary>
        /// Fires whenever stats may have changed. UI / debug surfaces subscribe and re-read; the engine flips
        /// the dirty flag and recomputes lazily on next <see cref="GetValue(uint)"/>.
        /// </summary>
        public event Action OnStatsDirty;

        protected virtual void NotifyDirty() {
            _isDirty = true;
            OnStatsDirty?.Invoke();
        }

        /// <summary>
        /// Get the base value for a stat before modifiers, or 0 if unset.
        /// </summary>
        public float GetBase(uint statHash) =>
                _baseValues.TryGetValue(statHash, out var value)
                        ? StatSettings.ToExternal(value)
                        : 0f;

        public bool HasBase(uint statHash) => _baseValues.ContainsKey(statHash);

        /// <summary>
        /// Add a modifier to this container; applied during the next calculation.
        /// </summary>
        public void AddModifier(StatModifierEntry modifierEntry) {
            if (!_modifiersByStatId.ContainsKey(modifierEntry.StatHash))
                _modifiersByStatId[modifierEntry.StatHash] = new List<StatModifierEntry>();

            _modifiersByStatId[modifierEntry.StatHash].Add(modifierEntry);
            NotifyDirty();
        }

        /// <summary>
        /// Remove all modifier entries carrying this unique id (unequip an item, remove a buff, etc.).
        /// Returns the number of entries removed; the behaviour is only dirtied when that count is non-zero.
        /// </summary>
        public int RemoveModifierByUniqueId(string uniqueId) {
            if (string.IsNullOrEmpty(uniqueId)) {
                Log.Error("Attempting to remove a modifier with a null ID.");

                return 0;
            }

            var removed = 0;

            foreach (var modifierList in _modifiersByStatId.Values)
                removed += modifierList.RemoveAll(m => m.UniqueId == uniqueId);

            if (removed > 0)
                NotifyDirty();

            return removed;
        }

        /// <summary>
        /// Get the final calculated value for a stat (base + all modifiers), recalculating if dirty.
        /// </summary>
        public float GetValue(uint statHash) {
            if (_isDirty)
                Recalculate();

            if (_calculatedValues.TryGetValue(statHash, out var value))
                return StatSettings.ToExternal(value);

            return GetBase(statHash);
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
        /// Every stat hash with a base value set.
        /// </summary>
        public IEnumerable<uint> StatHashes => _baseValues.Keys;

        #region Name-Based Overloads

        /// <summary>Name-keyed <see cref="SetBase(uint, float)"/>; hashes + validates the name via <see cref="StatRegistry"/>.</summary>
        public void SetBase(string statName, float value) => SetBase(StatRegistry.GetHash(statName), value);

        /// <summary>Name-keyed <see cref="GetBase(uint)"/>; hashes + validates the name via <see cref="StatRegistry"/>.</summary>
        public float GetBase(string statName) => GetBase(StatRegistry.GetHash(statName));

        /// <summary>Name-keyed <see cref="HasBase(uint)"/>; hashes + validates the name via <see cref="StatRegistry"/>.</summary>
        public bool HasBase(string statName) => HasBase(StatRegistry.GetHash(statName));

        /// <summary>Name-keyed <see cref="GetValue(uint)"/>; hashes + validates the name via <see cref="StatRegistry"/>.</summary>
        public float GetValue(string statName) => GetValue(StatRegistry.GetHash(statName));

        /// <summary>
        /// Add a <see cref="ModifierType.Flat"/> modifier to the named stat. The optional
        /// <paramref name="uniqueId"/> lets the caller later remove this exact modifier via
        /// <see cref="RemoveModifierByUniqueId"/>.
        /// </summary>
        public void AddFlat(string statName, float value, string uniqueId = null) =>
                AddModifier(new StatModifierEntry(
                    StatRegistry.GetHash(statName),
                    ModifierType.Flat,
                    value,
                    uniqueId));

        /// <summary>
        /// Add a <see cref="ModifierType.Increased"/> modifier (additive % pool) to the named stat.
        /// </summary>
        public void AddIncreased(string statName, float percent, string uniqueId = null) =>
                AddModifier(new StatModifierEntry(
                    StatRegistry.GetHash(statName),
                    ModifierType.Increased,
                    percent,
                    uniqueId));

        /// <summary>
        /// Add a <see cref="ModifierType.More"/> modifier (multiplicative chain) to the named stat.
        /// </summary>
        public void AddMore(string statName, float percent, string uniqueId = null) =>
                AddModifier(new StatModifierEntry(
                    StatRegistry.GetHash(statName),
                    ModifierType.More,
                    percent,
                    uniqueId));

        /// <summary>
        /// Add a <see cref="ModifierType.Override"/> modifier (first-Override-wins; ignores Base / Flat /
        /// Increased / More) to the named stat.
        /// </summary>
        public void AddOverride(string statName, float value, string uniqueId = null) =>
                AddModifier(new StatModifierEntry(
                    StatRegistry.GetHash(statName),
                    ModifierType.Override,
                    value,
                    uniqueId));

        #endregion

        #region Owned Stat Declaration

        /// <summary>
        /// The stats this behaviour owns plus the base value each ships with. Override in a concrete behaviour
        /// so the authoring inspector reveals exactly these (pre-filled, no orphans) and the runtime can seed
        /// them as a fallback. Empty by default — a behaviour with no declaration authors via the raw stats list.
        /// </summary>
        public virtual IReadOnlyList<StatAndValue> Declare() => Array.Empty<StatAndValue>();

        /// <summary>Builds a declared (stat, default-value) pair by name for use inside <see cref="Declare"/>.</summary>
        protected static StatAndValue Own(string statName, float defaultValue) =>
                new(StatRegistry.GetHash(statName), defaultValue);

        #endregion

        /// <summary>
        /// Recalculate all stats by applying modifiers in PoE order.
        /// </summary>
        private void Recalculate() {
            _calculatedValues.Clear();

            foreach (var (statHash, modifiers) in _modifiersByStatId)
                _calculatedValues[statHash] = CalculateStat(statHash, modifiers);

            _isDirty = false;
        }

        /// <summary>
        /// Calculate a single stat's final value by applying modifiers in PoE order, in fixed-point int math.
        /// </summary>
        private int CalculateStat(uint statHash, List<StatModifierEntry> modifiers) {
            var baseValue = _baseValues.GetValueOrDefault(statHash, 0);
            var precision = StatSettings.Precision;

            // Step 1: Apply flat modifiers
            var flatSum = 0;

            foreach (var mod in modifiers) {
                if (mod.Type == ModifierType.Flat)
                    flatSum += StatSettings.ToInternal(mod.Value);
            }

            var afterFlat = baseValue + flatSum;

            // Step 2: Apply all Increased modifiers - they stack additively
            var increasedSum = 0;

            foreach (var mod in modifiers) {
                if (mod.Type == ModifierType.Increased)
                    increasedSum += StatSettings.ToInternal(mod.Value);
            }

            var afterIncreased = (long)afterFlat * (precision + increasedSum) / precision;

            // Step 3: Apply all More modifiers - each is multiplicative
            var afterMore = afterIncreased;

            foreach (var mod in modifiers) {
                if (mod.Type == ModifierType.More) {
                    var moreValue = StatSettings.ToInternal(mod.Value);
                    afterMore = afterMore * (precision + moreValue) / precision;
                }
            }

            // Step 4: Check for Override modifiers (first one wins — CI-style "becomes X" effects
            // are not displaced by later overrides on the same stat)
            foreach (var mod in modifiers) {
                if (mod.Type == ModifierType.Override)
                    return StatSettings.ToInternal(mod.Value);
            }

            return (int)afterMore;
        }

        #region IPacker

        /// <summary>
        /// Pack the container as base values + flattened modifier list, keyed by stat hash (a stable 4-byte id).
        /// </summary>
        public void Pack(ref Span<byte> buffer) {
            Packer.WriteInt(ref buffer, _baseValues.Count);

            foreach (var (hash, value) in _baseValues) {
                Packer.WriteUInt(ref buffer, hash);
                Packer.WriteInt(ref buffer, value);
            }

            Packer.WriteInt(ref buffer, ModifierCount);

            foreach (var (hash, modifiers) in _modifiersByStatId) {
                foreach (var mod in modifiers) {
                    Packer.WriteUInt(ref buffer, hash);
                    Packer.WriteByte(ref buffer, (byte)mod.Type);
                    Packer.WriteFloat(ref buffer, mod.Value);
                    Packer.WriteString(ref buffer, mod.UniqueId ?? string.Empty);
                }
            }
        }

        /// <summary>
        /// Replace the container's contents from the buffer.
        /// </summary>
        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            Clear();

            var baseCount = Packer.ReadInt(ref buffer);

            for (var i = 0; i < baseCount; i++) {
                var hash = Packer.ReadUInt(ref buffer);
                var value = Packer.ReadInt(ref buffer);
                _baseValues[hash] = value;
            }

            var modCount = Packer.ReadInt(ref buffer);

            for (var i = 0; i < modCount; i++) {
                var hash = Packer.ReadUInt(ref buffer);
                var type = (ModifierType)Packer.ReadByte(ref buffer);
                var value = Packer.ReadFloat(ref buffer);
                var uniqueId = Packer.ReadString(ref buffer);

                if (!_modifiersByStatId.TryGetValue(hash, out var list)) {
                    list = new List<StatModifierEntry>();
                    _modifiersByStatId[hash] = list;
                }

                list.Add(new StatModifierEntry(hash, type, value, uniqueId));
            }

            _isDirty = true;
        }

        /// <summary>
        /// A deep copy — same concrete type, same base values + modifiers — via a Pack/Unpack round-trip. Gives
        /// each spawned target its own instance cloned from a shared authored composition.
        /// </summary>
        public virtual SbBehaviour Clone() {
            var clone = (SbBehaviour)Activator.CreateInstance(GetType());
            var payload = Packer.BuildPayload((ref Span<byte> buffer) => Pack(ref buffer));
            ReadOnlySpan<byte> span = payload;
            clone.Unpack(ref span);

            return clone;
        }

        #endregion

        #region Display Helpers

        /// <summary>
        /// A formatted string of all base stats for debug output.
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
        /// A formatted string of all calculated stats for debug output, recalculating if dirty.
        /// </summary>
        public string GetCalculatedStatList() {
            if (_isDirty)
                Recalculate();

            var allStatHashes = new HashSet<uint>(_baseValues.Keys);

            foreach (var statHash in _modifiersByStatId.Keys)
                allStatHashes.Add(statHash);

            var lines = allStatHashes
                    .Select(statHash => {
                        var name = StatRegistry.GetName(statHash) ?? $"Unknown({statHash})";
                        var baseValue = GetBase(statHash);
                        var finalValue = GetValue(statHash);

                        return $"  {name}: {finalValue:F2} (base: {baseValue})";
                    });

            return string.Join("\n", lines);
        }

        /// <summary>
        /// A detailed breakdown of how a stat's value is calculated.
        /// </summary>
        public string GetModifierAnalysis(uint statHash) {
            var statName = StatRegistry.GetName(statHash) ?? $"Unknown({statHash})";
            var baseValue = GetBase(statHash);

            var lines = new List<string> {
                $"Stat: {statName}",
                $"Base: {baseValue}"
            };

            if (!_modifiersByStatId.TryGetValue(statHash, out var modifiers) || modifiers.Count == 0) {
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
                lines.Add($"Override: {overrides[0]} (ignores all calculations)");

            lines.Add($"Final: {GetValue(statHash):F2}");

            return string.Join("\n", lines);
        }

        #endregion
    }
}
