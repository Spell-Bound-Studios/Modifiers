// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Stats {
    /// <summary>
    /// Implement on behaviours that need to hook into skill events.
    /// Called automatically when behaviour is added to a skill.
    /// </summary>
    public interface IEventAware {
        void Subscribe(Skill skill);
        void Unsubscribe(Skill skill);
    }
}