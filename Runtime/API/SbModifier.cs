// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Convenience base for the 80% modifier use case: bundles <see cref="IModifier"/> with a generated
    /// <see cref="IHasUniqueId.UniqueId"/>, a JSON-based default <see cref="Clone"/>, and protected helpers
    /// (<see cref="TryGetStats"/> / <see cref="TryGetBehaviour{T}"/> / <see cref="TryGetEvents"/>) that reach
    /// into the target's containers without callers writing the cast boilerplate.
    /// </summary>
    /// <remarks>
    /// Concrete subclasses are typically <c>[Serializable] sealed</c> so they can ride a
    /// <c>[SerializeReference]</c> field and appear in the <c>[DropdownPicker]</c> menu. Power users with
    /// their own identity or hierarchy needs implement <see cref="IModifier"/> + <see cref="IHasUniqueId"/>
    /// directly (the README's documented "20% power user" escape hatch).
    /// </remarks>
    [Serializable]
    public abstract class SbModifier : IModifier, IHasUniqueId {
        public abstract void Apply(ICanBeModified target);

        public abstract void Remove(ICanBeModified target);

        public string UniqueId { get; set; } = Guid.NewGuid().ToString();

        public virtual IModifier Clone() {
            var json = JsonUtility.ToJson(this);
            var clone = (SbModifier)JsonUtility.FromJson(json, GetType());
            clone.UniqueId = Guid.NewGuid().ToString();

            return clone;
        }

        #region Convenience Methods
        
        /// <summary>
        /// Attempts to get the SbBehaviour from the ICanBeModified target if an IHasBehaviour exists on the target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="behaviour"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        ///
        /// </returns>
        protected bool TryGetBehaviour<T>(ICanBeModified target, out T behaviour) where T : SbBehaviour {
            behaviour = null;

            return target is IHasBehaviours hb && hb.Behaviours.TryGetBehaviour(out behaviour);
        }

        /// <summary>
        /// Attempts to get the EventContainer from the ICanBeModified target if an IHasEvents exists on the target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="events"></param>
        /// <returns>
        ///
        /// </returns>
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