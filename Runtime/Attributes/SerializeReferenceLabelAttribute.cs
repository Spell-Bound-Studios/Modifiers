// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SerializeReferenceLabelAttribute : Attribute {
        public string Label { get; }

        public SerializeReferenceLabelAttribute(string label) => Label = label;
    }
}
