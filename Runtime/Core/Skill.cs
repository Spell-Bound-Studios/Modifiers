// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Base class for all skills.
    /// Skills have stats that can be modified and tags that determine which modifiers apply.
    /// </summary>
    public abstract class Skill : IModifiable, IStats {
        public string Name { get; protected set; }
        public HashSet<int> Tags { get; protected set; } = new();
        public StatContainer Stats { get; protected set; } = new();
        private readonly Dictionary<Type, IBehaviour> _behaviours = new();

        public void AddBehaviour<T>(T behaviour) where T : IBehaviour {
            _behaviours[typeof(T)] = behaviour;
        }
        
        public void RemoveBehaviour<T>() where T : IBehaviour {
            _behaviours.Remove(typeof(T));
        }
        
        public T GetBehaviour<T>() where T : IBehaviour {
            if (_behaviours.TryGetValue(typeof(T), out var behaviour))
                return (T)behaviour;
            
            return default;
        }
        
        public bool HasBehaviour<T>() where T : IBehaviour {
            return _behaviours.ContainsKey(typeof(T));
        }
        
        public void ApplyModifiers(IEnumerable<IModifier> modifiers) {
            foreach (var modifier in modifiers)
                modifier.Apply(this);
        }
        
        public virtual void Cast(object caster, object target) {
            Execute();
        }

        /// <summary>
        /// Execute all behaviours with the given context.
        /// </summary>
        protected virtual void Execute() {
            foreach (var behaviour in _behaviours.Values) {
                behaviour.Execute();
            }
        }
    }
}