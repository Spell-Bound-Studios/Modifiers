// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// Composability contract: "this target owns an <see cref="EventContainer"/>." Modifiers attach handlers
    /// here in <see cref="SbModifier.Apply"/> and detach them in <see cref="SbModifier.Remove"/>; the target
    /// is responsible for invoking the named events at the right moments (on-hit, on-cast, on-death, etc.).
    /// </summary>
    /// <example>
    /// if (target is not IHasEvents iEvents) return;
    /// </example>
    public interface IHasEvents {
        EventContainer Events { get; }
    }
}