// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored asset listing every <see cref="StatDefinition"/> the game knows about plus a global
    /// decimal precision. Calling <see cref="RegisterAll"/> at boot interns every name into
    /// <see cref="StatRegistry"/>, configures <see cref="StatSettings.Precision"/>, hands itself to
    /// <see cref="BehaviourExtensions"/> for pretty-printing, and (optionally) flips strict-validation on so
    /// any later <c>"foo"</c> typo throws instead of silently registering a phantom stat.
    /// </summary>
    /// <remarks>
    /// One database per game (or one master + add-on databases composed at boot). The drop-in
    /// <see cref="StatDatabaseLoader"/> handles this without code; <c>StatDemo</c> in the samples is the
    /// hand-written equivalent.
    /// </remarks>
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Stat Database")]
    public class StatDatabase : ScriptableObject {
        [Header("Settings"), SerializeField,
         Tooltip("Decimal precision for fixed-point math. 4 = ten-thousandths (default), 2 = hundredths")]
        private int decimalPrecision = 4;

        [Header("Stats"), SerializeField] private List<StatDefinition> stats = new();

        private Dictionary<string, StatDefinition> _lookup;

        public IReadOnlyList<StatDefinition> Stats => stats;

        public bool logVerbosely;

        /// <summary>
        /// Registers all stats and configures precision.
        /// </summary>
        public void RegisterAll(bool strictStatValidation = false) {
            StatSettings.SetDecimalPrecision(decimalPrecision);
            BehaviourExtensions.SetDatabase(this);

            _lookup = new Dictionary<string, StatDefinition>();

            foreach (var stat in stats) {
                if (stat == null)
                    continue;

                if (_lookup.ContainsKey(stat.StatName)) {
                    Log.Error($"[StatDatabase] Duplicate stat '{stat.StatName}' detected. Skipping.");

                    continue;
                }

                stat.Register();
                _lookup[stat.StatName] = stat;

                if (logVerbosely) Log.Debug($"Registered the stat '{stat.StatName}'.");
            }

            if (strictStatValidation)
                StatRegistry.EnableStrictValidation(_lookup.Keys);

            Log.Info(
                $"[StatDatabase] Registered {_lookup.Count} stats. Precision: {decimalPrecision} decimals. Strict validation: {strictStatValidation}");
        }

        public StatDefinition GetDefinition(string statName) {
            if (_lookup == null) {
                Log.Warn("[StatDatabase] GetDefinition called before RegisterAll()");

                return null;
            }

            _lookup.TryGetValue(statName, out var def);

            return def;
        }

        public bool IsValidStat(string statName) => _lookup != null && _lookup.ContainsKey(statName);

#if UNITY_EDITOR
        private void OnValidate() {
            var seen = new HashSet<StatDefinition>();
            var seenNames = new HashSet<string>();

            for (var i = 0; i < stats.Count; i++) {
                var stat = stats[i];

                if (stat == null)
                    continue;

                if (seen.Contains(stat)) {
                    Log.Warn($"[StatDatabase] Duplicate reference to '{stat.StatName}' at index {i}. " +
                             "Remove the duplicate.");
                }
                else if (seenNames.Contains(stat.StatName)) {
                    Log.Warn($"[StatDatabase] Duplicate stat name '{stat.StatName}' at index {i}. " +
                             "Two different assets have the same stat name.");
                }
                else {
                    seen.Add(stat);
                    seenNames.Add(stat.StatName);
                }
            }
        }
#endif
    }
}