// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core;
using UnityEngine;

namespace Spellbound.Stats {
    /// <summary>
    /// A centralized location for all the stats you want in your game. If you use this you will have an easier
    /// time using the editor tool because it will automatically register the strings you create and map them to their
    /// ID's so that you don't have to remember which ID belongs to which stat.
    /// </summary>
    [CreateAssetMenu(fileName = "StatDefinitions", menuName = "Spellbound/Stats/StatDefinitions")]
    public class StatDefinitions : ScriptableObject, IStatRegistry {
        [SerializeField] private List<StatDefinition> stats = new();
        
        [Serializable]
        public class StatDefinition {
            [Tooltip("Name of the stat")]
            public string name;
            
            [Tooltip("Auto-assigned ID")]
            [Immutable] public int id;
        }
        
#if UNITY_EDITOR
        private void OnValidate() {
            foreach (var stat in stats) {
                if (!string.IsNullOrEmpty(stat.name))
                    stat.id = StatRegistry.Register(stat.name);
            }
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        
        /// <summary>
        /// Get all stat names for dropdown usage.
        /// </summary>
        public IEnumerable<string> GetAllStatNames() {
            foreach (var stat in stats) {
                if (!string.IsNullOrEmpty(stat.name))
                    yield return stat.name;
            }
        }
        
        /// <summary>
        /// Get ID for a stat name.
        /// </summary>
        public int GetStatId(string statName) {
            foreach (var stat in stats) {
                if (stat.name == statName)
                    return stat.id;
            }
            return -1;
        }
        
        /// <summary>
        /// Register all stats in this Scriptable Object at runtime.
        /// Call this during game initialization.
        /// </summary>
        public void RegisterAll(bool verbose) {
            if (verbose)
                Debug.Log($"[StatDefinitions] Registering stats from '{name}'...");
    
            foreach (var stat in stats) {
                if (string.IsNullOrEmpty(stat.name)) 
                    continue;

                var id = StatRegistry.Register(stat.name);
            
                if (verbose)
                    Debug.Log($"    - {stat.name} (ID: {id})");
            }
    
            if (verbose)
                Debug.Log($"[StatDefinitions] Registered {stats.Count} stats.");
        }
    }
}