// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Sample stage: reflects a fraction of one incoming damage type straight back at the attacker, before any
    /// mitigation touches it. It sits at the front of the circuit so it reads the pre-absorption value, and it
    /// never changes <see cref="DamageContext.Incoming"/> — the defender still takes its full hit; reflect is a
    /// copy. The copy is dealt to the attacker with no attacker of its own, so it can't bounce back.
    /// </summary>
    [Serializable]
    public sealed class ReflectStage : IPipelineStage<DamageContext> {
        private readonly string _damageType;
        private readonly float _fraction;

        private uint? _typeHash;

        public ReflectStage(string damageType, float fraction) {
            _damageType = damageType;
            _fraction = fraction;
        }

        private uint TypeHash => _typeHash ??= StatRegistry.GetHash(_damageType);

        public void Process(in DamageContext ctx) {
            Log.Debug($"[Reflect] stage running: type={_damageType} fraction={_fraction} " +
                    $"attacker={(ctx.Attacker == null ? "NULL" : ctx.Attacker.name)} incomingCount={ctx.Incoming.Count}");

            if (ctx.Attacker == null) {
                Log.Debug("[Reflect] attacker is NULL — nothing to reflect to. Bailing.");

                return;
            }

            var reflected = 0f;

            foreach (var entry in ctx.Incoming) {
                Log.Debug($"[Reflect]   incoming hash={entry.statHash} amount={entry.amount} (want {_damageType}={TypeHash})");

                if (entry.statHash == TypeHash)
                    reflected += entry.amount * _fraction;
            }

            Log.Debug($"[Reflect] total reflected = {reflected}");

            if (reflected <= 0f) {
                Log.Debug("[Reflect] reflected <= 0 — nothing to send. Bailing.");

                return;
            }

            Log.Debug($"[Reflect] dealing {reflected} {_damageType} back to attacker '{ctx.Attacker.name}'");
            ctx.Attacker.TakeHit(new List<StatAndValue> { new(TypeHash, reflected) });
        }
    }
}
