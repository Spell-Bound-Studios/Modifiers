// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    public abstract class ModifiableObject : ICanBeModified, IHasStats, IHasBehaviours, IHasEvents {
        public abstract string Name { get; }
        public StatContainer Stats { get; } = new();
        public BehaviourContainer Behaviours { get; } = new();
        public EventContainer Events { get; } = new();
    
        public abstract void Initialize();
    }
}