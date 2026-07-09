// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Modifiers.Samples {
    /// <summary>
    /// An item archetype. Implicits are ONE list — each entry is a named ModifierDefinition or an inline
    /// contribution, chosen per entry in the inspector; both belong to the item and ride identical pathways.
    /// A fixed magnitude is identical on every instance; a rolled magnitude rolls once per instance, at the
    /// moment the instance is created (see <see cref="ItemInstance"/>). The pool holds the named modifiers
    /// instances of this item may roll on top.
    /// </summary>
    [CreateAssetMenu(menuName = "Spellbound/ModifierLib/Sample Item")]
    public sealed class ItemDefinition : ScriptableObject {
        [Header("Identity"), SerializeField] private string itemName;
        [SerializeField, TextArea] private string description;
        [Header("Implicits"), SerializeField] private ModifierGrantSet implicits = new();
        [Header("Modifiers"), SerializeField] private ModifierPool pool;

        public string ItemName => string.IsNullOrEmpty(itemName) ? name : itemName;
        public string Description => description;
        public ModifierGrantSet Implicits => implicits;
        public ModifierPool Pool => pool;
    }
}
