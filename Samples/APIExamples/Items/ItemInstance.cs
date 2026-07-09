// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Hashing;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// An <see cref="ItemDefinition"/> made real. Construction is the drop moment: implicits roll here, once,
    /// and belong to this instance for its whole life — persist ImplicitRolls and Modifiers to save or send
    /// it. Implicits and rolled modifiers ride identical pathways: contributions keyed by source id, applied
    /// on equip, stripped by RemoveSource on unequip.
    /// </summary>
    public sealed class ItemInstance {
        private static uint _instances;

        private readonly RolledGrants _implicitRolls;
        private readonly List<RolledModifier> _modifiers = new();
        private uint _modifierSequence;
        private Modifiable _wearer;

        public ItemInstance(ItemDefinition definition, System.Random rng) {
            Definition = definition;
            SourceId = StableHash.Fnv1A32($"{definition.name}_{++_instances}");
            _implicitRolls = definition.Implicits.Roll(rng, SourceId);
        }

        public ItemDefinition Definition { get; }
        public uint SourceId { get; }
        public RolledGrants ImplicitRolls => _implicitRolls;
        public IReadOnlyList<RolledModifier> Modifiers => _modifiers;
        public bool IsEquipped => _wearer != null;

        public void Equip(Modifiable target) {
            if (_wearer != null)
                return;

            _wearer = target;
            Definition.Implicits.Apply(target, SourceId, in _implicitRolls);

            for (var i = 0; i < _modifiers.Count; i++)
                _modifiers[i].TryApplyTo(target);
        }

        public void Unequip() {
            if (_wearer == null)
                return;

            _wearer.RemoveSource(SourceId);

            for (var i = 0; i < _modifiers.Count; i++)
                _modifiers[i].RemoveFrom(_wearer);

            _wearer = null;
        }

        public bool AddRandomModifier(System.Random rng) {
            if (Definition.Pool == null)
                return false;

            var picks = Definition.Pool.Sample(1, rng);

            if (picks.Count == 0)
                return false;

            var sourceId = StableHash.Fnv1A32($"{SourceId}_modifier_{++_modifierSequence}");
            var rolled = picks[0].Roll(rng, sourceId);
            _modifiers.Add(rolled);

            if (_wearer != null)
                rolled.TryApplyTo(_wearer);

            return true;
        }

        public bool RemoveRandomModifier(System.Random rng) {
            if (_modifiers.Count == 0)
                return false;

            var index = rng.Next(_modifiers.Count);

            if (_wearer != null)
                _modifiers[index].RemoveFrom(_wearer);

            _modifiers.RemoveAt(index);

            return true;
        }

        public int RemoveAllModifiers() {
            var removed = _modifiers.Count;

            if (_wearer != null) {
                for (var i = 0; i < _modifiers.Count; i++)
                    _modifiers[i].RemoveFrom(_wearer);
            }

            _modifiers.Clear();

            return removed;
        }
    }
}
