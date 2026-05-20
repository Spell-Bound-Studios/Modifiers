// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// All-in-one base for any in-game subject that wants the full lib stack: stats, behaviours, and events,
    /// targetable by modifiers. Subclass for skills, characters, items, world props — anything that should be
    /// stat-driven, capability-composed, and event-emitting. <see cref="Initialize"/> is the hook for wiring
    /// behaviours together (read a behaviour's event, fire another behaviour's method, etc.).
    /// </summary>
    /// <remarks>
    /// Implements all three composability contracts plus <see cref="ICanBeModified"/>, so modifier code can
    /// always reach the containers without reflection. Skills in the samples (see <c>Fireball</c> /
    /// <c>RayOfFrost</c> when they're re-added) are just <see cref="ModifiableObject"/>s that add behaviours
    /// in their constructor and wire them in <see cref="Initialize"/>.
    /// </remarks>
    public abstract class ModifiableObject : ICanBeModified, IHasStats, IHasBehaviours, IHasEvents {
        public abstract string Name { get; }
        public StatContainer Stats { get; } = new();
        public BehaviourContainer Behaviours { get; } = new();
        public EventContainer Events { get; } = new();

        public abstract void Initialize();
    }
}