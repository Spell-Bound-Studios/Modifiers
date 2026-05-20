// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Convention contract for "this thing emits a positional event" — e.g. a skill activation point, an
    /// AOE trigger, a portal spawn. The implementer owns invoking <see cref="OnPositionTriggered"/> at the
    /// right moment; subscribers (typically modifiers) handle the payload.
    /// </summary>
    /// <remarks>
    /// Pattern-only. The interface enforces nothing beyond a settable <see cref="Action{T}"/> property and
    /// could be replaced by a string-keyed event on <see cref="EventContainer"/> with no loss of capability.
    /// Kept as a typed surface for samples / discoverability; reconsider during the 1.0 API cleanup whether
    /// it earns its keep alongside the event-container path.
    /// </remarks>
    public interface ITriggersPositionalEvent {
        Action<PositionalPayload> OnPositionTriggered { get; set; }
    }
}