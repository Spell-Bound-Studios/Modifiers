using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Modifiers {
    [Serializable]
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Modded Collection")]
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