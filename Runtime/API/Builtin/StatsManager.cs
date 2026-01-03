// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats {
    /// <summary>
    /// Bootstraps the stats system at runtime by registering all definitions.
    /// Add this to your initialization scene and assign any IStatRegistry implementers.
    /// </summary>
    public class StatsManager : MonoBehaviour {
        [Tooltip("All registries to initialize (StatDefinitions, TagDefinitions, custom registries)")]
        [SerializeField] private List<Object> registries = new();
        
        [Tooltip("Log detailed registration information to unity console")]
        [SerializeField] private bool verbose;
        
        private void Awake() {
            if (verbose)
                Debug.Log("[StatsManager] Beginning registration...");
            
            foreach (var obj in registries) {
                if (obj is IStatRegistry registry)
                    registry.RegisterAll(verbose);
                else
                    Debug.LogWarning($"[StatsManager] ✗ {obj.name} does not implement IStatRegistry");
            }
            
            if (verbose)
                Debug.Log($"[StatsManager] Registration complete. {registries.Count} registries processed.");
        }
    }
}