// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Stats.Attributes {
    /// <summary>
    /// Mark an int field as a stat ID to get dropdown support in the inspector.
    /// Requires StatDefinitions ScriptableObject to be present, otherwise falls back to int field.
    /// </summary>
    public class StatIdAttribute : PropertyAttribute { }
}