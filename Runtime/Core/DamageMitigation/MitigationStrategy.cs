// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Abstract scriptable-object strategy for combining an incoming damage magnitude with a defensive stat
    /// value, producing the mitigated damage. Subclass and add <see cref="CreateAssetMenuAttribute"/> to ship
    /// any formula a game wants — percent reduction, armor formula, flat reduction, conditional curves,
    /// whatever. The lib only ships the simplest default; everything else is user-defined.
    /// </summary>
    /// <remarks>
    /// Input convention: <paramref name="incomingDamage"/> is always a positive magnitude (the *magnitude* of
    /// damage about to land, not the negative delta-channel value). Strategies return the mitigated positive
    /// magnitude; the calling stage handles re-negating into delta form.
    /// </remarks>
    public abstract class MitigationStrategy : ScriptableObject {
        public abstract float Apply(float incomingDamage, float defensiveStatValue);
    }
}