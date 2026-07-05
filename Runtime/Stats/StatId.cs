// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    public readonly struct StatId : IEquatable<StatId> {
        public readonly uint Hash;

        public StatId(uint hash) {
            Hash = hash;
        }

        public static StatId From(string name) => new(StatRegistry.GetHash(name));

        public static implicit operator uint(StatId id) => id.Hash;

        public override string ToString() => StatRegistry.TryGetName(Hash, out var n) ? n : $"#{Hash}";

        #region IEquatable Implementation

        public bool Equals(StatId other) => Hash == other.Hash;
        public override bool Equals(object obj) => obj is StatId other && Equals(other);
        public override int GetHashCode() => (int)Hash;

        public static bool operator ==(StatId a, StatId b) => a.Hash == b.Hash;
        public static bool operator !=(StatId a, StatId b) => a.Hash != b.Hash;

        #endregion IEquatable Implementation
    }
}