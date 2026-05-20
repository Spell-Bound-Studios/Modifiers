// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// One entry inside a <see cref="StatSlice"/>: a stat's runtime id paired with its computed value at the
    /// time the slice was crafted. Ids (not names) ride the wire because slices are ephemeral tick DTOs and
    /// the registry is deterministic when both ends share the same loaded <see cref="StatDatabase"/>.
    /// </summary>
    [Serializable]
    public struct StatSliceEntry {
        public int id;
        public float value;

        public StatSliceEntry(int id, float value) {
            this.id = id;
            this.value = value;
        }

        public override string ToString() => $"#{id}, {StatRegistry.GetName(id)}: {value:F2}";
    }
}