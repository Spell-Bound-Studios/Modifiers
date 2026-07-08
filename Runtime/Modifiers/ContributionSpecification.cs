// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers {
    [Serializable]
    public sealed class ContributionSpecification {
        [SerializeField] private StatDefinition stat;
        [SerializeField] private ContributionType type;
        [SerializeReference] private Magnitude magnitude;
        [SerializeField] private StatDefinition pairedStat;
        [SerializeReference] private Magnitude pairedMagnitude;
        [SerializeField] private bool linkOrdered;

        public StatDefinition Stat => stat;
        public ContributionType Type => type;
        public Magnitude Magnitude => magnitude;
        public StatDefinition PairedStat => pairedStat;
        public Magnitude PairedMagnitude => pairedMagnitude;
        public bool LinkOrdered => linkOrdered;

        public bool IsValid =>
                stat != null && magnitude != null && magnitude.IsValid &&
                (pairedStat == null || (pairedStat != stat && pairedMagnitude != null && pairedMagnitude.IsValid));
    }
}
