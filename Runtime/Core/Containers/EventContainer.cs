// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;

namespace Spellbound.Modifiers {
    /// <summary>
    /// String-keyed typed event bus owned by one target. Modifiers attach handlers in <see cref="SbModifier.Apply"/>
    /// and detach them in <see cref="SbModifier.Remove"/>; the target's own code invokes events at the right
    /// moment (on-hit, on-cast, on-tick, on-death, on-damage-taken, …). Payload type is whatever the publisher
    /// chose — typically a struct like <see cref="TargetedPayload"/> or <see cref="PositionalPayload"/>.
    /// </summary>
    /// <remarks>
    /// Type-checks at invocation time: if a handler was registered as <c>Action&lt;A&gt;</c> but invoked with a
    /// <c>B</c> payload, the container logs a warning and skips rather than throwing. Lets samples / modifiers
    /// stay loose without crashing the host on a typo.
    /// </remarks>
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
                    Log.Warn(
                        $"[EventContainer] Type mismatch on event '{name}'. Expected {del.GetType()}, got Action<{typeof(T)}>.");

                    break;
            }
        }

        public void Set<T>(string name, Action<T> handler) => _events[name] = handler;

        public void ClearSingle(string name) {
            if (_events.ContainsKey(name))
                _events[name] = null;
        }

        public void ClearAll() => _events.Clear();

        public bool HasEvent(string name) => _events.ContainsKey(name);

        public bool HasHandlers(string name) => _events.TryGetValue(name, out var del) && del != null;

        public IEnumerable<string> GetEventNames() => _events.Keys;

        public int Count => _events.Count;
    }
}