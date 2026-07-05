// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// The canonical affix: one item, any number of stat contributions, all under one source id. Every
    /// "+X to stat" item in the demo is an instance of this — items are data, not subclasses.
    /// </summary>
    public sealed class StatItem : ModifiableItem {
        private readonly (StatId stat, ContributionType type, float value)[] _entries;

        public StatItem(params (StatId stat, ContributionType type, float value)[] entries) => _entries = entries;

        protected override void OnEquip(Modifiable target) {
            foreach (var (stat, type, value) in _entries)
                target.Stats.AddContribution(stat, type, value, SourceId);
        }
    }
}
