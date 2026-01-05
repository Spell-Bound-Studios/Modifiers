// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Spellbound.Stats {
    /// <summary>
    /// Runtime instance of a skill with mutable stats, tags, and behaviours.
    /// Created from a Skill ObjectPreset, then modified by player build.
    /// </summary>
    public class Skill : ICanBeModified, IStats {
        public string Name { get; set; }
        public StatContainer Stats { get; private set; } = new();
        private readonly Dictionary<Type, IBehaviour> _behaviours = new();
        public HashSet<int> Tags => _behaviours.Values
                .SelectMany(b => b.Tags)
                .ToHashSet();
        
        public Skill(string name) {
            Name = name;
        }

        public void AddBehaviour(IBehaviour behaviour) => _behaviours[behaviour.GetType()] = behaviour;
        public void RemoveBehaviour<T>() where T : IBehaviour => _behaviours.Remove(typeof(T));
        public T GetBehaviour<T>() where T : IBehaviour =>
                _behaviours.TryGetValue(typeof(T), out var behaviour) 
                    ? (T)behaviour 
                    : default;
        public bool HasBehaviour<T>() where T : IBehaviour => _behaviours.ContainsKey(typeof(T));
    }
}