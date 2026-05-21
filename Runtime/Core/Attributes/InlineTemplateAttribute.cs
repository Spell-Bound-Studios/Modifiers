// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Marker for a <see cref="SerializableAttribute"/> struct that should render as a compact one-row entry
    /// when it appears as the element type of a serialized list on a preset module. The generic
    /// <c>PresetModuleDrawer</c> detects this attribute and lays out the struct's serialized fields
    /// horizontally with move/remove buttons instead of the default expanded-foldout view.
    /// </summary>
    /// <remarks>
    /// Any new template-like struct can opt in by adding this attribute — no drawer code change required.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class InlineTemplateAttribute : Attribute { }
}