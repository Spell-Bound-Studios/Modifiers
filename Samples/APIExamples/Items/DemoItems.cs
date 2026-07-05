// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Hashing;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Base of every demo item: one instance-unique source id — the handle every stat contribution and circuit
    /// grant is keyed under, and the single thing RemoveSource needs to strip it all back out.
    /// </summary>
    public abstract class DemoItem {
        private static uint _instances;

        protected DemoItem() {
            SourceId = StableHash.Fnv1A32($"{GetType().Name}_{++_instances}");
        }

        public uint SourceId { get; }
    }

    /// <summary>
    /// An item that modifies any <see cref="Modifiable"/>. Unequip is uniform for every subclass:
    /// RemoveSource strips each stat contribution and circuit grant this item made — no per-item bookkeeping.
    /// </summary>
    public abstract class ModifiableItem : DemoItem {
        public void Equip(Modifiable target) => OnEquip(target);

        public void Unequip(Modifiable target) => target.RemoveSource(SourceId);

        protected abstract void OnEquip(Modifiable target);
    }
}