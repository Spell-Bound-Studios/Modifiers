// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    public sealed class LevelController : MonoBehaviour {
        private readonly List<RolledModifier> _rolled = new();
        private uint _nextSourceId = 1;

        public Modifiable Modifiable { get; } = new();
        public string RolledIcons { get; private set; } = "";
        public IReadOnlyList<RolledModifier> Rolled => _rolled;

        public void Reroll(ModifierPool pool, int count, System.Random rng) {
            foreach (var modifier in _rolled)
                modifier.RemoveFrom(Modifiable);

            _rolled.Clear();

            if (pool != null) {
                foreach (var definition in pool.Sample(count, rng))
                    _rolled.Add(definition.Roll(rng, _nextSourceId++));
            }

            foreach (var modifier in _rolled)
                modifier.TryApplyTo(Modifiable);

            RolledIcons = CombatColors.ModifierIcons(_rolled);
        }
    }
}