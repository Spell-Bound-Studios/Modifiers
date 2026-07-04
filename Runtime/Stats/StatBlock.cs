// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;

namespace Spellbound.Modifiers {
    public sealed class StatBlock {
        public event Action<StatId> Changed;

        private readonly Dictionary<StatId, int> _base = new();
        private readonly Dictionary<StatId, List<Contribution>> _mods = new();
        private readonly Dictionary<StatId, Accumulator> _staticCache = new();
        private readonly Dictionary<uint, HashSet<StatId>> _bySource = new();
        private readonly HashSet<StatId> _dirty = new();
        private readonly HashSet<StatId> _resolving = new();

        #region Base values

        public void SetBase(StatId stat, float value) {
            _base[stat] = StatSettings.ToInternal(value);
            MarkChanged(stat);
        }

        public float GetBase(StatId stat) =>
                _base.TryGetValue(stat, out var value) ? StatSettings.ToExternal(value) : 0f;

        public bool HasBase(StatId stat) => _base.ContainsKey(stat);

        #endregion Base values

        #region Modifiers

        public void AddModifier(
                StatId stat, ModifierType type, float value, uint sourceId = Contribution.Innate,
                Condition condition = null) {
            if (!_mods.TryGetValue(stat, out var list)) {
                list = new List<Contribution>();
                _mods[stat] = list;
            }

            list.Add(Contribution.Of(type, value, sourceId, condition));

            if (sourceId != Contribution.Innate) {
                if (!_bySource.TryGetValue(sourceId, out var stats)) {
                    stats = new HashSet<StatId>();
                    _bySource[sourceId] = stats;
                }

                stats.Add(stat);
            }

            MarkChanged(stat);
        }

        public int RemoveBySource(uint sourceId) {
            if (sourceId == Contribution.Innate) {
                Log.Error("Attempting to remove innate contributions (source id 0).");

                return 0;
            }

            if (!_bySource.Remove(sourceId, out var stats))
                return 0;

            var removed = 0;

            foreach (var stat in stats) {
                if (!_mods.TryGetValue(stat, out var list))
                    continue;

                removed += list.RemoveAll(c => c.SourceId == sourceId);
                MarkChanged(stat);
            }

            return removed;
        }

        public void Clear() {
            _base.Clear();
            _mods.Clear();
            _staticCache.Clear();
            _bySource.Clear();
            _dirty.Clear();
        }

        #endregion Modifiers

        #region Resolution

        public float GetValue(StatId stat, CircuitContext ctx) {
            if (_dirty.Remove(stat))
                RebuildStat(stat);

            var accumulator = _staticCache.TryGetValue(stat, out var cached) ? cached : new Accumulator();
            var baseInternal = _base.TryGetValue(stat, out var b) ? b : 0;

            if (!_resolving.Add(stat)) {
                Log.Error($"Stat condition cycle detected while resolving '{stat}'. " +
                          "Conditional modifiers were skipped for this read.");

                return StatSettings.ToExternal(accumulator.Resolve(baseInternal));
            }

            try {
                if (_mods.TryGetValue(stat, out var list)) {
                    for (var i = 0; i < list.Count; i++) {
                        var contribution = list[i];

                        if (contribution.IsConditional && contribution.Condition.Met(ctx))
                            accumulator.Apply(contribution.Type, contribution.ValueInternal);
                    }
                }
            }
            finally {
                _resolving.Remove(stat);
            }

            return StatSettings.ToExternal(accumulator.Resolve(baseInternal));
        }

        private void RebuildStat(StatId stat) {
            if (!_mods.TryGetValue(stat, out var list)) {
                _staticCache.Remove(stat);

                return;
            }

            var accumulator = new Accumulator();

            for (var i = 0; i < list.Count; i++) {
                var contribution = list[i];

                if (!contribution.IsConditional)
                    accumulator.Apply(contribution.Type, contribution.ValueInternal);
            }

            _staticCache[stat] = accumulator;
        }

        private void MarkChanged(StatId stat) {
            _dirty.Add(stat);
            Changed?.Invoke(stat);
        }

        #endregion Resolution
    }
}
