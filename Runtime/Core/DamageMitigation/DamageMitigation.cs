// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Modifiers {
    /// <summary>
    /// One row in a mitigation mapping: which damage stat is the incoming channel, which defensive stat
    /// reduces it, and which <see cref="MitigationStrategy"/> performs the math. Multiple rows may share the
    /// same <see cref="defensiveStat"/> (e.g. pierce / blunt / slash all map to <c>armor</c>) and rows can
    /// freely mix strategies (e.g. elemental rows use percent reduction, physical rows use an armor formula).
    /// </summary>
    [Serializable]
    public struct DamageMitigation {
        public StatDefinition damageStat;
        public StatDefinition defensiveStat;
        public MitigationStrategy strategy;
    }
}