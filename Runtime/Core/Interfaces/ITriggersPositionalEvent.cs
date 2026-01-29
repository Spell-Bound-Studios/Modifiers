using System;

namespace Spellbound.Stats {
    /// <summary>
    /// Implement this on anything that triggers at a position.
    /// The implementer is responsible for invoking OnPositionTriggered when the position is triggered.
    /// </summary>
    public interface ITriggersPositionalEvent {
        Action<PositionalPayload> OnPositionTriggered { get; set; }
    }
}