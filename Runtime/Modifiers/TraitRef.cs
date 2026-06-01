// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Inline <see cref="SbModifier"/> wrapper around a <see cref="Trait"/> asset reference.
    /// Lets <c>[SerializeReference] List&lt;SbModifier&gt;</c> fields hold a mix of inline
    /// <c>Affix</c> instances AND references to Trait assets — both are SbModifier subclasses,
    /// both flow through the same Apply / Remove machinery, but TraitRef delegates to the
    /// asset's Effect on apply.
    /// </summary>
    /// <remarks>
    /// <para>Apply clones the asset's Effect and applies the clone to the target, retaining the
    /// clone so Remove can tear it down cleanly. The clone gets its own UniqueId so multiple
    /// references to the same Trait stack correctly.</para>
    /// <para>Pack / Unpack store only the Trait's hashed id (4 bytes); on unpack, the asset is
    /// resolved via <see cref="TraitRegistry"/>. The asset itself never travels over the wire or
    /// into save data.</para>
    /// </remarks>
    [Serializable]
    public sealed class TraitRef : SbModifier {
        [Tooltip("The Trait asset this reference points at. Apply clones the asset's Effect."), SerializeField]
        private Trait trait;

        // Runtime — the cloned modifier we applied, so Remove can undo it. Not serialized.
        private SbModifier _appliedClone;

        public Trait Trait => trait;

        /// <summary>
        /// Configure at runtime — used by pool sampling and any other code path that needs to
        /// construct a TraitRef fresh. Returns this for chaining. Inspector-authored instances
        /// assign <c>trait</c> via SerializeField directly.
        /// </summary>
        public TraitRef Initialize(Trait trait) {
            this.trait = trait;

            return this;
        }

        public override void Apply(ICanBeModified target) {
            if (trait == null || trait.Effect == null)
                return;

            if (_appliedClone != null)
                Log.Warn(
                    $"TraitRef.Apply called twice without an intervening Remove (trait='{trait.Key}'); the previous clone is being orphaned on the target.");

            _appliedClone = (SbModifier)trait.Effect.Clone();
            _appliedClone.Apply(target);
        }

        public override void Remove(ICanBeModified target) {
            _appliedClone?.Remove(target);
            _appliedClone = null;
        }

        public override void Pack(ref Span<byte> buffer) {
            var id = trait != null ? TraitRegistry.Hash(trait.Key) : 0u;
            Packer.WriteUInt(ref buffer, id);
        }

        public override void Unpack(ref ReadOnlySpan<byte> buffer) {
            var id = Packer.ReadUInt(ref buffer);
            trait = TraitRegistry.GetById(id);
        }
    }
}