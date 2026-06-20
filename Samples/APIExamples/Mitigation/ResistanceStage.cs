// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Percent-reduces one damage type by its matching resistance (5 = 5% off), capped at 100%. One stage, three
    /// instances — fire / cold / lightning behave identically, just on different stats. Reads the resistance by
    /// name off the context, so it never knows which behaviour owns it.
    /// </summary>
    [Serializable]
    public sealed class ResistanceStage : IPipelineStage<DamageContext> {
        private readonly string _resistanceStat;
        private readonly string _damageStat;

        private uint? _resistanceHash;
        private uint? _damageHash;

        public ResistanceStage(string resistanceStat, string damageStat) {
            _resistanceStat = resistanceStat;
            _damageStat = damageStat;
        }

        private uint ResistanceHash => _resistanceHash ??= StatRegistry.GetHash(_resistanceStat);
        private uint DamageHash => _damageHash ??= StatRegistry.GetHash(_damageStat);

        public void Process(in DamageContext ctx) {
            var resistance = ctx.GetValue(ResistanceHash);

            if (resistance <= 0f)
                return;

            var multiplier = Math.Max(0f, 1f - resistance / 100f);

            for (var i = 0; i < ctx.Incoming.Count; i++) {
                var entry = ctx.Incoming[i];

                if (entry.statHash != DamageHash)
                    continue;

                ctx.Incoming[i] = new StatAndValue(entry.statHash, entry.amount * multiplier);
            }
        }
    }
}
