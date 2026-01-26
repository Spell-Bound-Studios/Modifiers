// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats {
    /// <summary>
    /// Container for events. Handles registration, subscription, and invocation.
    /// </summary>
    public class EventContainer {
        private readonly Dictionary<string, Delegate> _events = new();
        
        public void Add<T>(string name, Action<T> handler) {
            if (_events.TryGetValue(name, out var existing))
                _events[name] = Delegate.Combine(existing, handler);
            else
                _events[name] = handler;
            
        }
        
        public void Remove<T>(string name, Action<T> handler) {
            if (_events.TryGetValue(name, out var existing))
                _events[name] = Delegate.Remove(existing, handler);
        }
        
        public void Invoke<T>(string name, T payload) {
            if (!_events.TryGetValue(name, out var del))
                return;
            
            switch (del) {
                // If it doesn't exist then silently handle.
                case null:
                    return;
                case Action<T> handler:
                    handler.Invoke(payload);
                    break;
                // If it's the wrong one entirely then warn.
                default:
                    Debug.LogWarning($"[EventContainer] Type mismatch on event '{name}'. Expected {del.GetType()}, got Action<{typeof(T)}>.");
                    break;
            }
        }
        
        public void Set<T>(string name, Action<T> handler) =>
            _events[name] = handler;

        public void ClearSingle(string name) {
            if (_events.ContainsKey(name))
                _events[name] = null;
        }

        public void ClearAll() => _events.Clear();

        public bool HasEvent(string name) => _events.ContainsKey(name);

        public bool HasHandlers(string name) =>
            _events.TryGetValue(name, out var del) && del != null;

        public IEnumerable<string> GetEventNames() => _events.Keys;

        public int Count => _events.Count;
    }
}