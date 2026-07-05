// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// Spawns a world-space floating combat number at a world position. Self-contained — anchored in the world,
    /// no HUD or screen projection involved.
    /// </summary>
    public static class CombatText {
        public static void Pop(Vector3 worldPosition, float amount, Color color) {
            var obj = new GameObject("CombatNumber") { transform = { position = worldPosition } };
            obj.AddComponent<FloatingNumber>().Init(amount, color);
        }
    }
}