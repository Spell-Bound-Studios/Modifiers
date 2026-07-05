// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// The structural item: grants a <see cref="ReflectLeaf"/> into the target's Convert stage, where it reads
    /// pre-mitigation values. Unequip is inherited — RemoveSource pulls the grant back out of the circuit.
    /// </summary>
    public sealed class ReflectFireItem : ModifiableItem {
        private readonly float _fraction;

        public ReflectFireItem(float fraction = 0.25f) {
            _fraction = fraction;
        }

        protected override void OnEquip(Modifiable target) {
            var circuit = target.CircuitFor(DemoEvents.TakeHit);

            if (circuit.TryGetStage(DemoStages.Convert, out var convert))
                convert.Add(new ReflectLeaf(DemoStats.FireDamage, _fraction, DemoConsequences.ReflectedFire), 0,
                    SourceId);
        }
    }
}