// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers {
    [Serializable]
    [SerializeReferenceLabel("Scale from a stat")]
    public sealed class DerivedMagnitude : Magnitude {
        [SerializeReference, Tooltip("The amount granted per unit of the source stat (see Per Points).")]
        private ScalarMagnitude amount;

        [SerializeField, Tooltip("Grant the amount once for every this-many points of the source stat. 20 = per 20 Intelligence.")]
        private int perPoints = 1;

        [SerializeField, Tooltip("On: only full breakpoints count (55 at Per 20 = 2). Off: scales smoothly.")]
        private bool stepped;

        [SerializeField, Tooltip("The stat that drives the scaling (e.g. Intelligence).")]
        private StatDefinition source;

        [SerializeField, Tooltip("Whose stat to read: Owner (this entity) or Subject (the querying skill/child).")]
        private Perspective perspective;

        public override bool IsValid => amount != null && amount.IsValid && source != null;

        public override bool Rolls => amount != null && amount.Rolls;

        public override float Bake(System.Random rng) => amount != null ? amount.Bake(rng) : 0f;

        public override void ApplyTo(StatBlock stats, StatId stat, ContributionType type, uint sourceId, float baked) {
            if (amount == null || source == null)
                return;

            stats.AddDerived(stat, type, new StatId(source.Hash), amount.Value(baked), perPoints, stepped, perspective,
                    sourceId);
        }
    }
}
