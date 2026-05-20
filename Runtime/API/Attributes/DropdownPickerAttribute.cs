// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Marks a serialized field as picker-driven in the inspector. On <c>[SerializeReference]</c> fields the
    /// drawer offers every concrete <c>[Serializable]</c> implementer of the field's interface / base type;
    /// on <c>ObjectReference</c> fields it offers every matching ScriptableObject asset in the project; on
    /// <c>List&lt;T&gt;</c> / <c>T[]</c> it does the same per element with add / remove / reorder buttons.
    /// Implementation: <c>Editor/DropdownPickerDrawer.cs</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DropdownPickerAttribute : PropertyAttribute { }
}