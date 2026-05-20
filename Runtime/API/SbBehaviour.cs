// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Base class for a pure capability. A behaviour knows HOW to do exactly one thing (fire a projectile,
    /// receive damage, emit a beam, hold a resource pool, run a duration effect) and owns its own
    /// <see cref="StatContainer"/> for the numbers that govern that thing. It does NOT know when it runs,
    /// what triggers it, or what comes after — that orchestration is the GAME's job (see the README).
    /// </summary>
    /// <remarks>
    /// Subclass and override <see cref="InitializeStats"/> to seed base values; everything else (cooldowns,
    /// triggers, FX, networking) is layered on by the consuming game. The <see cref="SerializableAttribute"/>
    /// is required so concrete subclasses can ride a <c>[SerializeReference]</c> field for designer authoring.
    /// </remarks>
    [Serializable]
    public abstract class SbBehaviour {
        private StatContainer _stats;
        public StatContainer Stats => _stats ??= InitializeStats();

        protected virtual StatContainer InitializeStats() => new();
    }
}