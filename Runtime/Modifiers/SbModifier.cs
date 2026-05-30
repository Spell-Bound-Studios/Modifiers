// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Convenience base for the 80% modifier use case: bundles <see cref="IModifier"/> with a generated
    /// <see cref="IHasUniqueId.UniqueId"/>, an <see cref="IPacker"/>-based default <see cref="Clone"/>, and
    /// protected helpers (<see cref="TryGetBehaviour{T}"/> / <see cref="TryGetEvents"/>) that reach into
    /// the target's containers without callers writing the cast boilerplate.
    /// </summary>
    /// <remarks>
    /// Concrete subclasses are typically <c>[Serializable] sealed</c> so they can ride a
    /// <c>[SerializeReference]</c> field and appear in the <c>[DropdownPicker]</c> menu. Power users with
    /// their own identity or hierarchy needs implement <see cref="IModifier"/> + <see cref="IHasUniqueId"/>
    /// directly (the README's documented "20% power user" escape hatch).
    /// </remarks>
    [Serializable]
    public abstract class SbModifier : IModifier, IHasUniqueId, IPacker {
        public abstract void Apply(ICanBeModified target);

        public abstract void Remove(ICanBeModified target);

        public string UniqueId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Write this modifier's state into <paramref name="buffer"/>. Concrete subclasses pack
        /// whatever they need to round-trip an equivalent instance — typically a few primitives.
        /// Stateless modifiers (IronWill, ImmuneToFireDamage) leave the body empty. UniqueId is
        /// NOT packed: it's a process-local apply/remove handle, freshly generated on each
        /// constructed instance.
        /// </summary>
        public abstract void Pack(ref Span<byte> buffer);

        /// <summary>
        /// Read state from <paramref name="buffer"/> into this instance. Mirror of <see cref="Pack"/>.
        /// </summary>
        public abstract void Unpack(ref ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Deep-clone via the project's binary packer. Round-trips the modifier through
        /// <see cref="Packer.ToBytes{T}"/> + <see cref="Activator.CreateInstance(Type)"/> +
        /// <see cref="Unpack"/>, then stamps a fresh <see cref="UniqueId"/>. Concrete subclasses
        /// must expose a parameterless constructor for the <c>Activator.CreateInstance</c> path.
        /// </summary>
        public virtual IModifier Clone() {
            var bytes = Packer.ToBytes(this);
            ReadOnlySpan<byte> span = bytes;
            var clone = (SbModifier)Activator.CreateInstance(GetType());
            clone.Unpack(ref span);
            clone.UniqueId = Guid.NewGuid().ToString();

            return clone;
        }

        #region Convenience Methods

        /// <summary>
        /// Attempts to get the SbBehaviour from the ICanBeModified target if an IHasBehaviour exists on the target.
        /// </summary>
        protected bool TryGetBehaviour<T>(ICanBeModified target, out T behaviour) where T : SbBehaviour {
            behaviour = null;

            return target is IHasBehaviours hb && hb.Behaviours.TryGetBehaviour(out behaviour);
        }

        /// <summary>
        /// Attempts to get the EventContainer from the ICanBeModified target if an IHasEvents exists on the target.
        /// </summary>
        protected bool TryGetEvents(ICanBeModified target, out EventContainer events) {
            events = null;

            if (target is not IHasEvents he)
                return false;

            events = he.Events;

            return true;
        }

        #endregion
    }
}