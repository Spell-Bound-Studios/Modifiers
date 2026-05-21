// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Per-target storage for <see cref="SbBehaviour"/> instances, keyed by concrete type. One entry per
    /// behaviour subclass; <see cref="Add"/> overwrites silently. This is the surface modifiers reach for when
    /// they want to read or alter a target's capability (e.g. <c>TryGetBehaviour&lt;ProjectileBehaviour&gt;</c>
    /// then <c>stats.AddFlat("projectile_count", …)</c>).
    /// </summary>
    /// <remarks>
    /// Behaviours are the libraries "what this thing can do" vocabulary — projectile-firing, beam-emitting,
    /// damage-receiving, resource-pooling. A target may own as many as the game wants; the container is just
    /// the typed bag.
    /// <para>
    /// Unity-serializable: the <c>behaviours</c> list carries <see cref="DropdownPickerAttribute"/> so
    /// designers pick concrete subclasses from a dropdown when this container is a <c>[SerializeField]</c>
    /// on a <see cref="MonoBehaviour"/> or <see cref="ScriptableObject"/>.
    /// <see cref="ISerializationCallbackReceiver.OnAfterDeserialize"/> mirrors that list into the runtime
    /// <c>_lookup</c> dictionary so type-keyed reads stay O(1). Runtime <see cref="Add"/> /
    /// <see cref="Remove{T}"/> mutate the dictionary only — they are intentionally NOT written back into the
    /// serialized list, so a transient buff added during playmode does not stick to the scene asset on save.
    /// </para>
    /// </remarks>
    [Serializable]
    public class BehaviourContainer : ISerializationCallbackReceiver {
        private readonly Dictionary<Type, SbBehaviour> _lookup = new();

        public void Add(SbBehaviour behaviour) => _lookup[behaviour.GetType()] = behaviour;

        public void Remove<T>() where T : SbBehaviour => _lookup.Remove(typeof(T));

        /// <summary>
        /// Trys to get a value from the dictionary. Can return null.
        /// </summary>
        /// <typeparam name="T">
        /// Behaviours are expected to be SbBehaviour types.
        /// </typeparam>
        public T GetBehaviour<T>() where T : SbBehaviour =>
                _lookup.TryGetValue(typeof(T), out var b)
                        ? (T)b
                        : null;

        /// <summary>
        /// Trys to get a value from the behaviour dictionary and returns a bool based on success.
        /// </summary>
        /// <param name="behaviour">Behaviours are expected to be SbBehaviour types.</param>
        /// <typeparam name="T">Outs an SbBehaviour type if bool is true but otherwise outs behaviour as null.</typeparam>
        /// <returns>True or false.</returns>
        public bool TryGetBehaviour<T>(out T behaviour) where T : SbBehaviour {
            if (_lookup.TryGetValue(typeof(T), out var b)) {
                behaviour = (T)b;

                return true;
            }

            behaviour = null;

            return false;
        }

        public IEnumerable<SbBehaviour> GetAll() => _lookup.Values;

        public IEnumerable<T> GetAll<T>() => _lookup.Values.OfType<T>();

        public void Clear() => _lookup.Clear();

        public int Count => _lookup.Count;

        #region Inspector Authoring

        // This list and the ISerializationCallbackReceiver methods below exist ONLY because Unity can't
        // serialize the _lookup dictionary directly. Nothing in runtime gameplay code reads or writes this
        // list — it's the persisted-and-inspector-editable form of _lookup, bridged into the dictionary on
        // load. Runtime mutations to _lookup are intentionally NOT mirrored back, so transient buffs added
        // during playmode don't stick to the scene asset on save.

        [SerializeReference, DropdownPicker(typeof(PickableBehaviourAttribute))]
        private List<SbBehaviour> behaviours = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            _lookup.Clear();

            foreach (var b in behaviours)
                if (b != null)
                    _lookup[b.GetType()] = b;
        }

        #endregion
    }
}
