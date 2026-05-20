// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Marks an <see cref="SbBehaviour"/> subclass as a pipeline stage and binds it to a concrete context type.
    /// The <see cref="PipelineStageRegistry"/> uses this attribute to discover stages, and editor pickers filter
    /// to the matching <see cref="ContextType"/> so a `DamageReceiveContext` stage never appears in a
    /// `DamageOutgoingContext` config.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PipelineStageAttribute : Attribute {
        public Type ContextType { get; }
        public string DisplayName { get; }
        public string Description { get; }

        public PipelineStageAttribute(Type contextType, string displayName = null, string description = null) {
            ContextType = contextType;
            DisplayName = displayName;
            Description = description;
        }
    }
}