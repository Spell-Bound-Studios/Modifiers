// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Container for events. Handles registration, subscription, and invocation.
    /// </summary>
    public class EventContainer {
        private readonly Dictionary<string, Delegate> _events = new();
        
        public void Register<T>(string name) {
            _events.TryAdd(name, null);
        }
        
        public void Add<T>(string name, Action<T> handler) {
            if (_events.TryGetValue(name, out var existing)) {
                _events[name] = Delegate.Combine(existing, handler);
            } else {
                _events[name] = handler;
            }
        }
        
        public void Remove<T>(string name, Action<T> handler) {
            if (_events.TryGetValue(name, out var existing)) {
                _events[name] = Delegate.Remove(existing, handler);
            }
        }
        
        public void Invoke<T>(string name, T payload) {
            if (_events.TryGetValue(name, out var del) && del is Action<T> handler) {
                handler.Invoke(payload);
            }
        }
        
        public bool HasEvent(string name) => _events.ContainsKey(name);
    }
}