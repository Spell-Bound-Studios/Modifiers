using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Implement this on anything that triggers at a position.
    /// The implementer is responsible for invoking OnPositionTriggered when the position is triggered.
    /// </summary>
    public interface ITriggersPositionalEvent {
        Action<PositionalPayload> OnPositionTriggered { get; set; }
    }
}