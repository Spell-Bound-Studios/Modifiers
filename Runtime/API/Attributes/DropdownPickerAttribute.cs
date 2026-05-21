// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Marks a serialized field as picker-driven in the inspector. On <c>[SerializeReference]</c> fields the
    /// drawer offers every concrete <c>[Serializable]</c> implementer of the field's interface / base type;
    /// on <c>ObjectReference</c> fields it offers every matching ScriptableObject asset in the project; on
    /// <c>List&lt;T&gt;</c> / <c>T[]</c> it does the same per element with add / remove / reorder buttons.
    /// Optionally pass a class-level <see cref="Attribute"/> type to <see cref="RequiredAttribute"/> to
    /// restrict the picker to types tagged with that attribute (e.g.
    /// <c>[DropdownPicker(typeof(PickableBehaviourAttribute))]</c> only lists pickable behaviours).
    /// Implementation: <c>Editor/DropdownPickerDrawer.cs</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DropdownPickerAttribute : PropertyAttribute {
        public Type RequiredAttribute { get; }

        public DropdownPickerAttribute() { }

        public DropdownPickerAttribute(Type requiredAttribute) {
            RequiredAttribute = requiredAttribute;
        }
    }
}
