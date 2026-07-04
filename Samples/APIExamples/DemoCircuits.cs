// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// The standard take-hit pipeline every demo entity shares: four pre-defined stages (an empty stage costs
    /// nothing and gives later modifiers somewhere to land), stat-driven mitigation leaves, and a terminal
    /// drain into the owner's life pool. Entity-specific defenses are granted in afterwards — the enemy's
    /// shield is just another grant into Mitigate at <see cref="ShieldPriority"/>.
    /// </summary>
    public static class DemoCircuits {
        public const int ShieldPriority = -10;
        public const int ResistancePriority = 0;
        public const int ArmorPriority = 10;

        public static Circuit BuildTakeHit(Modifiable modifiable, Action<float> drainLife) {
            var circuit = modifiable.CircuitFor(DemoEvents.TakeHit);

            var convert = circuit.DefineStage(DemoStages.Convert);
            var mitigate = circuit.DefineStage(DemoStages.Mitigate);
            var apply = circuit.DefineStage(DemoStages.Apply);
            var react = circuit.DefineStage(DemoStages.React);

            circuit.Root = new Sequence(convert, mitigate, apply, react);

            mitigate.Add(new ResistanceLeaf(DemoStats.FireDamage, DemoStats.FireResistance), ResistancePriority);
            mitigate.Add(new ResistanceLeaf(DemoStats.ColdDamage, DemoStats.ColdResistance), ResistancePriority);
            mitigate.Add(new ResistanceLeaf(DemoStats.LightningDamage, DemoStats.LightningResistance), ResistancePriority);
            mitigate.Add(new ArmorLeaf(DemoStats.PhysicalDamage, DemoStats.Armor), ArmorPriority);

            apply.Add(new ApplyToLifeLeaf(drainLife));

            return circuit;
        }
    }
}
