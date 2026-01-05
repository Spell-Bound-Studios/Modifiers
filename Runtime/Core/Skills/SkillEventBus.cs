// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Stats {
    /// <summary>
    /// Type-safe event bus scoped to a single skill instance.
    /// Behaviours and modifiers register handlers here.
    /// </summary>
    public class SkillEventBus {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();
        
        /// <summary>
        /// Subscribe to an event type.
        /// </summary>
        public void On<TEvent>(Action<TEvent> handler) where TEvent : struct, ISkillEvent {
            var type = typeof(TEvent);
            if (!_handlers.TryGetValue(type, out var list)) {
                list = new List<Delegate>();
                _handlers[type] = list;
            }
            list.Add(handler);
        }
        
        /// <summary>
        /// Unsubscribe from an event type.
        /// </summary>
        public void Off<TEvent>(Action<TEvent> handler) where TEvent : struct, ISkillEvent {
            if (_handlers.TryGetValue(typeof(TEvent), out var list))
                list.Remove(handler);
        }
        
        /// <summary>
        /// Trigger an event, notifying all subscribers.
        /// </summary>
        public void Trigger<TEvent>(TEvent evt) where TEvent : struct, ISkillEvent {
            if (!_handlers.TryGetValue(typeof(TEvent), out var list))
                return;
            
            // Iterate backwards to allow removal during iteration
            for (var i = list.Count - 1; i >= 0; i--) {
                if (list[i] is Action<TEvent> handler)
                    handler(evt);
            }
        }
        
        /// <summary>
        /// Clear all handlers (for cleanup).
        /// </summary>
        public void Clear() => _handlers.Clear();
    }
}