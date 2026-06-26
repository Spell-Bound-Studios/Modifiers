// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Flat-reduces every <c>physical_damage</c> entry by the defender's <c>armor</c>, clamped at zero. Reads
    /// armor by name off the context.
    /// </summary>
    [Serializable]
    public sealed class ArmorStage : IPipelineStage<DamageContext> {
        private static uint? _armorHash;
        private static uint? _physicalHash;

        private static uint ArmorHash => _armorHash ??= StatRegistry.GetHash("sample_armor");
        private static uint PhysicalHash => _physicalHash ??= StatRegistry.GetHash("sample_physical_damage");

        public void Process(in DamageContext ctx) {
            var armor = ctx.GetValue(ArmorHash);

            if (armor <= 0f)
                return;

            for (var i = 0; i < ctx.Incoming.Count; i++) {
                var entry = ctx.Incoming[i];

                if (entry.statHash != PhysicalHash)
                    continue;

                ctx.Incoming[i] = new StatAndValue(entry.statHash, Math.Max(0f, entry.amount - armor));
            }
        }
    }
}
