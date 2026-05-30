// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Abstract base for inline, anonymous, stat-flavor <see cref="SbModifier"/>s — the "+9 armor" /
    /// "+25% increased life" workhorse. Owns the data shape (stat / modifier type / value) and the
    /// <see cref="IPacker"/> round-trip; concrete subclasses define <c>Apply</c> / <c>Remove</c>
    /// with game-specific routing (which <see cref="SbBehaviour"/> on the target receives the
    /// modifier).
    /// </summary>
    /// <remarks>
    /// <para>Authored inline via <c>[SerializeReference]</c> on items, talents, and pool slots. No
    /// identity, no asset, no display name — the tooltip composer formats directly from the
    /// referenced stat. For named, registered identities (Iron Will, Thick Hide, etc.), use
    /// <see cref="Trait"/> + <see cref="TraitRef"/> instead.</para>
    /// <para><b>Why abstract:</b> stat modifiers must route into some specific
    /// <see cref="SbBehaviour"/> on the target (every behaviour holds its own stats). The library
    /// doesn't know about consumer-side behaviour subclasses, so the routing choice is the concrete
    /// subclass's job. A typical game ships <b>one</b> concrete Affix; if the routing layer needs to
    /// vary, it varies inside that concrete's Apply / Remove — never by adding more Affix
    /// subclasses. Routing-by-subclass leaks the target's behaviour topology into the authoring
    /// surface.</para>
    /// </remarks>
    [Serializable]
    public abstract class Affix : SbModifier {
        [SerializeField] private StatDefinition stat;
        [SerializeField] private ModifierType modifierType = ModifierType.Flat;
        [SerializeField] private float value;

        public StatDefinition Stat => stat;
        public ModifierType ModifierType => modifierType;
        public float Value => value;

        /// <summary>
        /// Mutator used by the <see cref="AffixExtensions.Initialize{T}"/> extension. Inspector-
        /// authored instances populate their fields via SerializeField; runtime construction
        /// flows through the extension (which returns the concrete subclass for fluent chaining).
        /// </summary>
        internal void SetData(StatDefinition stat, ModifierType modifierType, float value) {
            this.stat = stat;
            this.modifierType = modifierType;
            this.value = value;
        }

        public override void Pack(ref Span<byte> buffer) {
            Packer.WriteString(ref buffer, stat != null ? stat.StatName : string.Empty);
            Packer.WriteByte(ref buffer, (byte)modifierType);
            Packer.WriteFloat(ref buffer, value);
        }

        public override void Unpack(ref ReadOnlySpan<byte> buffer) {
            var statName = Packer.ReadString(ref buffer);

            if (string.IsNullOrEmpty(statName)) {
                stat = null;
            }
            else {
                stat = StatDefinitionRegistry.GetByName(statName);

                if (stat == null)
                    Log.Warn($"Affix.Unpack: stat '{statName}' is not registered in StatDefinitionRegistry; affix will no-op on Apply.");
            }

            modifierType = (ModifierType)Packer.ReadByte(ref buffer);
            value = Packer.ReadFloat(ref buffer);
        }
    }

    /// <summary>
    /// Extension layer for <see cref="Affix"/>. <see cref="Initialize{T}"/> returns the concrete
    /// subclass type via type inference from the receiver — fluent chains like
    /// <c>new StatAffix().Initialize(...)</c> retain the concrete type without a cast.
    /// </summary>
    public static class AffixExtensions {
        /// <summary>
        /// Configure an Affix at runtime — used by pool sampling and any other code path that
        /// constructs an Affix fresh with a rolled value. Returns the concrete subclass for
        /// fluent chaining. Inspector-authored instances assign their fields directly via
        /// SerializeField and don't need this.
        /// </summary>
        public static T Initialize<T>(this T affix, StatDefinition stat, ModifierType modifierType, float value)
                where T : Affix {
            affix.SetData(stat, modifierType, value);

            return affix;
        }
    }
}
