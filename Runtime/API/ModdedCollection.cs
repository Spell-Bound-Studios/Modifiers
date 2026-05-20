// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Designer-authored ScriptableObject that pairs one <see cref="ICanBeModified"/> with a list of
    /// <see cref="IModifier"/>s and acts as a factory: <see cref="CreateInstance"/> clones the modifiable,
    /// runs <see cref="ModifiableObject.Initialize"/> if applicable, then clones-and-applies every modifier.
    /// Both fields use <see cref="DropdownPickerAttribute"/>, so the inspector offers a typed dropdown of all
    /// serializable implementers — no manual subclass registration.
    /// </summary>
    /// <remarks>
    /// The cloning passes through <see cref="JsonUtility"/>, which means inspector-authored fields survive
    /// but anything held only as a runtime reference (managed objects, prefab links assigned at runtime, etc.)
    /// will not. Use it for fully-data-driven content; reach for code-built objects (see the sample
    /// <c>Fireball</c> pattern) when you need richer construction.
    /// </remarks>
    [Serializable, CreateAssetMenu(menuName = "Spellbound/ModifierLib/Modded Collection")]
    public class ModdedCollection : ScriptableObject {
        [SerializeReference, DropdownPicker] public ICanBeModified modifiableObject;
        [SerializeReference, DropdownPicker] public List<IModifier> modifiers;

        public ICanBeModified CreateInstance() {
            if (modifiableObject == null)
                return null;

            var json = JsonUtility.ToJson(modifiableObject);
            var instance = (ICanBeModified)JsonUtility.FromJson(json, modifiableObject.GetType());

            if (instance is ModifiableObject mo)
                mo.Initialize();

            foreach (var mod in modifiers) {
                var cloned = mod.Clone();
                cloned.Apply(instance);
            }

            return instance;
        }
    }
}