// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;
using System.Collections.Generic;

namespace Spellbound.Stats.Samples {
    /// <summary>
    /// Example item component showing how to use ModifierSlots.
    /// Designer configures modifiers in the inspector.
    /// </summary>
    public class ExampleItemComponent : MonoBehaviour {
        [Header("Item Properties")]
        [SerializeField] private string itemName = "Berserker Rage Helm";
        
        [Header("Modifiers")]
        [SerializeField] private List<ModifierSlot> modifierSlots = new();

        // This is me being lazy... you would probably have a class somewhere else that registers all of your modifiers.
        private void Awake() {
            ModifierRegistry.Register<ManaRecoupModifierDef>();
            StatRegistry.Register("mana_recoup");
        }

        /// <summary>
        /// Get all modifiers from this item's slots.
        /// Call this when the item is equipped.
        /// Note: Each slot can create MULTIPLE modifiers.
        /// </summary>
        public IEnumerable<IModifier> GetModifiers() {
            foreach (var slot in modifierSlots) {
                // Each slot can create 1 or more modifiers
                foreach (var modifier in slot.CreateModifiers(GetInstanceID())) {
                    yield return modifier;
                }
            }
        }

        // Example: When item is equipped
        public void OnEquip(PlayerTemplate player) {
            foreach (var modifier in GetModifiers())
                modifier.Apply(player);
            
            Debug.Log($"Equipped {itemName} with {modifierSlots.Count} modifiers");
        }
    }
}