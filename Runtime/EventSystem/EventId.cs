// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Registries;

namespace Spellbound.Modifiers {
    /// <summary>
    /// One registered event identity: a stable FNV-1a hash of its name. The hash is the save / wire-safe id an
    /// <see cref="EventContainer"/> keys by; the name is retained only for readable logs and authoring.
    /// </summary>
    public sealed class EventId : IRegistryEntry {
        public uint Hash { get; }
        public string Name { get; }

        public EventId(uint hash, string name) {
            Hash = hash;
            Name = name;
        }

        public override string ToString() => Name;
    }
}
