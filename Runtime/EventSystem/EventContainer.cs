// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;

namespace Spellbound.Modifiers {
    /// <summary>
    /// A hash-keyed typed event bus. Handlers register against an event id (a hash from
    /// <see cref="EventRegistry"/>) and are invoked with a payload whose type the publisher and subscriber
    /// agree on. Invoking an event with no handlers is a no-op, which lets a publisher gate work behind
    /// <see cref="HasHandlers"/> and pay nothing for an event nobody listens to.
    /// </summary>
    public class EventContainer {
        private readonly Dictionary<uint, Delegate> _events = new();

        public void Add<T>(uint eventId, Action<T> handler) {
            if (_events.TryGetValue(eventId, out var existing))
                _events[eventId] = Delegate.Combine(existing, handler);
            else
                _events[eventId] = handler;
        }

        public void Remove<T>(uint eventId, Action<T> handler) {
            if (_events.TryGetValue(eventId, out var existing))
                _events[eventId] = Delegate.Remove(existing, handler);
        }

        public void Invoke<T>(uint eventId, T payload) {
            if (!_events.TryGetValue(eventId, out var del) || del == null)
                return;

            if (del is Action<T> handler) {
                handler.Invoke(payload);

                return;
            }

            var name = EventRegistry.TryGetName(eventId, out var n) ? n : $"#{eventId}";
            Log.Warn($"[EventContainer] Type mismatch on event '{name}'. " +
                     $"Expected {del.GetType()}, got Action<{typeof(T)}>.");
        }

        public bool HasHandlers(uint eventId) => _events.TryGetValue(eventId, out var del) && del != null;

        public void ClearAll() => _events.Clear();
    }
}
