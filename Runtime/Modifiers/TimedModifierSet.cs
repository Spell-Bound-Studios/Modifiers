// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Modifiers {
    public sealed class TimedModifierSet {
        private readonly Modifiable _target;
        private readonly List<TimedModifier> _active = new();

        public event Action Changed;

        public TimedModifierSet(Modifiable target) => _target = target;

        public IReadOnlyList<TimedModifier> Active => _active;

        public void Apply(RolledModifier modifier, float duration) {
            var entry = new TimedModifier { modifier = modifier, duration = duration, remaining = duration };

            for (var i = 0; i < _active.Count; i++) {
                if (_active[i].modifier.modifierHash != modifier.modifierHash)
                    continue;

                _active[i].modifier.RemoveFrom(_target);
                modifier.TryApplyTo(_target);
                _active[i] = entry;
                Changed?.Invoke();

                return;
            }

            modifier.TryApplyTo(_target);
            _active.Add(entry);
            Changed?.Invoke();
        }

        public void Restore(TimedModifier entry) {
            entry.modifier.TryApplyTo(_target);
            _active.Add(entry);
            Changed?.Invoke();
        }

        public void Tick(float deltaTime) {
            var expired = false;

            for (var i = _active.Count - 1; i >= 0; i--) {
                var entry = _active[i];
                entry.remaining -= deltaTime;

                if (entry.remaining > 0f) {
                    _active[i] = entry;

                    continue;
                }

                entry.modifier.RemoveFrom(_target);
                _active.RemoveAt(i);
                expired = true;
            }

            if (expired)
                Changed?.Invoke();
        }

        public int Dispel(uint modifierHash) {
            var removed = 0;

            for (var i = _active.Count - 1; i >= 0; i--) {
                if (_active[i].modifier.modifierHash != modifierHash)
                    continue;

                _active[i].modifier.RemoveFrom(_target);
                _active.RemoveAt(i);
                removed++;
            }

            if (removed > 0)
                Changed?.Invoke();

            return removed;
        }

        public void Clear() {
            if (_active.Count == 0)
                return;

            for (var i = 0; i < _active.Count; i++)
                _active[i].modifier.RemoveFrom(_target);

            _active.Clear();
            Changed?.Invoke();
        }
    }
}
