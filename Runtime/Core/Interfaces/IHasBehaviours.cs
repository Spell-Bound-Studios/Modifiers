// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Modifiers {
    /// <summary>
    /// Composability contract: "this target owns a <see cref="BehaviourContainer"/>." Modifiers cast through
    /// this to reach a target's behaviours (see <see cref="SbModifier.TryGetBehaviour{T}"/>). Implement on
    /// anything that can carry capabilities — characters, weapons, projectiles, scene objects, anything.
    /// </summary>
    /// <example>
    /// if (target is not IHasBehaviours iBehaviours) return;
    /// </example>
    public interface IHasBehaviours {
        BehaviourContainer Behaviours { get; }
    }
}