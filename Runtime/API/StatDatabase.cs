using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats {
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Stat Database")]
    public class StatDatabase : ScriptableObject {
        [Header("Settings")]
        [SerializeField, Tooltip("Decimal precision for fixed-point math. 4 = ten-thousandths (default), 2 = hundredths")]
        private int decimalPrecision = 4;
        
        [Header("Stats")]
        [SerializeField] private List<StatDefinition> stats = new();
        
        private Dictionary<string, StatDefinition> _lookup;
        
        public IReadOnlyList<StatDefinition> Stats => stats;
        
        /// <summary>
        /// Registers all stats and configures precision.
        /// </summary>
        public void RegisterAll(bool strictStatValidation = false) {
            StatSettings.SetDecimalPrecision(decimalPrecision);
            ContainerExtensions.SetDatabase(this);
    
            _lookup = new Dictionary<string, StatDefinition>();
    
            foreach (var stat in stats) {
                if (stat == null)
                    continue;
        
                if (_lookup.ContainsKey(stat.StatName)) {
                    Debug.LogError($"[StatDatabase] Duplicate stat '{stat.StatName}' detected. Skipping.");
                    continue;
                }
        
                stat.Register();
                _lookup[stat.StatName] = stat;
            }
    
            if (strictStatValidation)
                StatRegistry.EnableStrictValidation(_lookup.Keys);
    
            Debug.Log($"[StatDatabase] Registered {_lookup.Count} stats. Precision: {decimalPrecision} decimals. Strict validation: {strictStatValidation}");
        }
        
        public StatDefinition GetDefinition(string statName) {
            if (_lookup == null) {
                Debug.LogWarning("[StatDatabase] GetDefinition called before RegisterAll()");
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
                    Debug.LogWarning($"[StatDatabase] Duplicate reference to '{stat.StatName}' at index {i}. Remove the duplicate.");
                }
                else if (seenNames.Contains(stat.StatName)) {
                    Debug.LogWarning($"[StatDatabase] Duplicate stat name '{stat.StatName}' at index {i}. Two different assets have the same stat name.");
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