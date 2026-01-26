// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Spellbound.Stats {
    /// <summary>
    /// Container for behaviours. This is what modifiers target.
    /// </summary>
    public class BehaviourContainer {
        private readonly Dictionary<Type, SbBehaviour> _behaviours = new();
        
        public void Add(SbBehaviour behaviour) => 
                _behaviours[behaviour.GetType()] = behaviour;
        
        public void Remove<T>() where T : SbBehaviour => 
                _behaviours.Remove(typeof(T));
        
        /// <summary>
        /// Trys to get a value from the dictionary. Can return null.
        /// </summary>
        /// <typeparam name="T">
        /// Behaviours are expected to be SbBehaviour types.
        /// </typeparam>
        public T GetBehaviour<T>() where T : SbBehaviour =>
                _behaviours.TryGetValue(typeof(T), out var b) 
                        ? (T)b 
                        : null;
        
        /// <summary>
        /// Trys to get a value from the behaviour dictionary and returns a bool based on success.
        /// </summary>
        /// <param name="behaviour">Behaviours are expected to be SbBehaviour types.</param>
        /// <typeparam name="T">Outs an SbBehaviour type if bool is true but otherwise outs behaviour as null.</typeparam>
        /// <returns>True or false.</returns>
        public bool TryGetBehaviour<T>(out T behaviour) where T : SbBehaviour {
            if (_behaviours.TryGetValue(typeof(T), out var b)) {
                behaviour = (T)b;
                return true;
            }
            behaviour = null;
            return false;
        }
        
        public IEnumerable<SbBehaviour> GetAll() => _behaviours.Values;
        
        public IEnumerable<T> GetAll<T>() => _behaviours.Values.OfType<T>();
        
        public void Clear() => _behaviours.Clear();
        
        public int Count => _behaviours.Count;
    }
}