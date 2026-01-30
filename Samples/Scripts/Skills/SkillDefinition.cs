using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Stats {
    /// <summary>
    /// Example showing how users build their own SO.
    /// </summary>
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Skill Definition")]
    public class SkillDefinition : ScriptableObject {
        [Header("Identity")]
        public string skillName;
        public Sprite icon;
    
        [Header("Visuals")]
        public GameObject projectilePrefab;
    
        [Header("Behaviours")]
        [SerializeReference, DropdownPicker] public List<SbBehaviour> behaviours = new();
    
        [Header("Base Modifiers")]
        [SerializeReference, DropdownPicker] public List<IModifier> modifiers = new();
    
        // public ModifiableObject CreateInstance() {
        //     var instance = new ModifiableObject();
        //
        //     foreach (var behaviour in behaviours)
        //         instance.Behaviours.Add(behaviour.Clone());
        //
        //     foreach (var mod in modifiers) {
        //         var modInstance = mod.Clone();
        //         modInstance.Apply(instance);
        //     }
        //
        //     return instance;
        // }
    }
}
