// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Packing;

namespace Spellbound.Modifiers {
    public interface IRolledModifier : ISmartPacker {
        uint SourceId { get; }

        bool TryApplyTo(Modifiable target);

        int RemoveFrom(Modifiable target);
    }
}
