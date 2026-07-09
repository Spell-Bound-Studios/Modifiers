// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Modifiers {
    /// <summary>
    /// One authored entry in a <see cref="ModifierGrantSet"/>: a named <see cref="ModifierDefinition"/> or any
    /// <see cref="ContributionSpecification"/> shape, chosen per entry in the inspector. Everything rolls at
    /// the owning instance's creation and applies through the same source-id pathways — the choice is
    /// authoring vocabulary, not a different system. Extend this class (or ContributionSpecification for pure
    /// stat lines) to add new kinds; the inspector picker lists subclasses automatically.
    /// </summary>
    [Serializable]
    public abstract class ModifierGrant {
        public abstract bool IsValid { get; }

        public abstract void Roll(
            System.Random rng, uint sourceId, List<BakedRoll> baked, List<RolledModifier> modifiers);

        public abstract void Apply(Modifiable target, uint sourceId, in RolledGrants rolled);
    }
}
