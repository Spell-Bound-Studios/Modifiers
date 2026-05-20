// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Convention contract for "this thing hits other things" — projectiles, beams, melee swings, ground
    /// patches. The implementer owns invoking <see cref="OnTargetHit"/> when a target is struck; subscribers
    /// (typically modifiers) react to the payload.
    /// </summary>
    /// <remarks>
    /// Pattern-only. Same observation as <see cref="ITriggersPositionalEvent"/>: this could be a string-keyed
    /// event on <see cref="EventContainer"/> without losing capability. Worth reconsidering whether the two
    /// <c>ITriggers*</c> interfaces add value over the generic event bus during the 1.0 API cleanup.
    /// </remarks>
    public interface ITriggersTargetedEvent {
        Action<TargetedPayload> OnTargetHit { get; set; }
    }
}