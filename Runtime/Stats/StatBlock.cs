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

        public void AddContribution(
            StatId stat, ContributionType type, float value, uint sourceId = Contribution.Innate,
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

        public void AddDerived(
            StatId stat, ContributionType type, StatId sourceStat, float ratioPerPoint,
            uint sourceId = Contribution.Innate, Condition condition = null) {
            if (sourceStat.Hash == 0) {
                Log.Error($"AddDerived on '{stat}' requires a source stat; nothing added.");

                return;
            }

            if (sourceStat.Hash == stat.Hash) {
                Log.Error($"AddDerived on '{stat}' cannot derive from itself; nothing added.");

                return;
            }

            if (!_mods.TryGetValue(stat, out var list)) {
                list = new List<Contribution>();
                _mods[stat] = list;
            }

            list.Add(Contribution.Derived(type, sourceStat, ratioPerPoint, sourceId, condition));

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

        public void Accumulate(StatId stat, CircuitContext ctx, ref Accumulator accumulator) {
            if (_dirty.Remove(stat))
                RebuildStat(stat);

            if (_staticCache.TryGetValue(stat, out var cached))
                accumulator.Merge(cached);

            if (!_resolving.Add(stat)) {
                Log.Error($"Stat dependency cycle detected while resolving '{stat}'. " +
                          "Conditional and derived modifiers were skipped for this read.");

                return;
            }

            try {
                if (_mods.TryGetValue(stat, out var list)) {
                    for (var i = 0; i < list.Count; i++) {
                        var contribution = list[i];

                        if (!contribution.IsConditional && !contribution.IsDerived)
                            continue;

                        if (contribution.IsConditional && !contribution.Condition.Met(ctx))
                            continue;

                        if (!contribution.IsDerived) {
                            accumulator.Apply(contribution.Type, contribution.ValueInternal);

                            continue;
                        }

                        var owner = ctx.Owner ?? ctx.Subject;

                        if (owner == null)
                            continue;

                        var sourceValue = owner.GetValue(contribution.SourceStat, ctx);
                        accumulator.Apply(contribution.Type,
                                StatSettings.ToInternal(sourceValue * contribution.RatioPerPoint));
                    }
                }
            }
            finally {
                _resolving.Remove(stat);
            }
        }

        public bool TryGetBaseInternal(StatId stat, out int baseInternal) => _base.TryGetValue(stat, out baseInternal);

        private void RebuildStat(StatId stat) {
            if (!_mods.TryGetValue(stat, out var list)) {
                _staticCache.Remove(stat);

                return;
            }

            var accumulator = new Accumulator();

            for (var i = 0; i < list.Count; i++) {
                var contribution = list[i];

                if (!contribution.IsConditional && !contribution.IsDerived)
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