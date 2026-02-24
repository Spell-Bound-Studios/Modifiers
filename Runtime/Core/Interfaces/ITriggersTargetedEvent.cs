using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Implement this on anything that "hits" things.
    /// The implementer is responsible for invoking OnTargetHit when a target is hit.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public interface ITriggersTargetedEvent {
        Action<TargetedPayload> OnTargetHit { get; set; }
    }
}