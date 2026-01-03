// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Stats.Attributes {
    /// <summary>
    /// Mark an int field as a tag ID to get dropdown support in the inspector.
    /// Requires TagDefinitions ScriptableObject to be present, otherwise falls back to int field.
    /// </summary>
    public class TagIdAttribute : PropertyAttribute { }
}