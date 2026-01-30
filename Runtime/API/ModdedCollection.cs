using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats {
    [Serializable]
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Modded Collection")]
    public class ModdedCollection : ScriptableObject {
        [SerializeReference, DropdownPicker] public ICanBeModified modifiableObject;
        [SerializeReference, DropdownPicker] public List<IModifier> modifiers;

        public ICanBeModified CreateInstance() {
            if (modifiableObject == null)
                return null;
            
            var type = modifiableObject.GetType();
            var instance = (ICanBeModified)Activator.CreateInstance(type);
            
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