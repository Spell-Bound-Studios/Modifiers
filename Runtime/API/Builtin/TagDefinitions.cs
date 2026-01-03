// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core;
using UnityEngine;

namespace Spellbound.Stats {
    /// <summary>
    /// A centralized location for all the tags you want in your game. If you use this you will have an easier
    /// time using the editor tool because it will automatically register the strings you create and map them to their
    /// ID's so that you don't have to remember which ID belongs to which.
    /// </summary>
    [CreateAssetMenu(fileName = "TagDefinitions", menuName = "Spellbound/Stats/TagDefinitions")]
    public class TagDefinitions : ScriptableObject, IStatRegistry {
        [SerializeField] private List<TagDefinition> tags = new();
    
        [Serializable]
        public class TagDefinition {
            [Tooltip("Name of the tag")]
            public string name;
            
            [Tooltip("Auto-assigned ID")]
            [Immutable] public int id;
        }
    
#if UNITY_EDITOR
        private void OnValidate() {
            foreach (var tag in tags) {
                if (!string.IsNullOrEmpty(tag.name))
                    tag.id = TagRegistry.Register(tag.name);
            }
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        
        /// <summary>
        /// Get all tag names for dropdown usage.
        /// </summary>
        public IEnumerable<string> GetAllTagNames() {
            foreach (var tag in tags) {
                if (!string.IsNullOrEmpty(tag.name))
                    yield return tag.name;
            }
        }
        
        /// <summary>
        /// Get ID for a tag name.
        /// </summary>
        public int GetTagId(string tagName) {
            foreach (var tag in tags) {
                if (tag.name == tagName)
                    return tag.id;
            }
            return -1;
        }
        
        /// <summary>
        /// Register all tags in this ScriptableObject at runtime.
        /// Call this during game initialization.
        /// </summary>
        public void RegisterAll(bool verbose) {
            if (verbose)
                Debug.Log($"[TagDefinitions] Registering tags from '{name}'...");
    
            foreach (var tag in tags) {
                if (string.IsNullOrEmpty(tag.name)) 
                    continue;

                var id = TagRegistry.Register(tag.name);
            
                if (verbose)
                    Debug.Log($"    - {tag.name} (ID: {id})");
            }
    
            if (verbose)
                Debug.Log($"[TagDefinitions] Registered {tags.Count} tags.");
        }
    }
}