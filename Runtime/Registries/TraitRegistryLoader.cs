// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Drop-in component that eagerly initializes <see cref="TraitRegistry"/> at startup. Add it
    /// to any GameObject in your bootstrap scene so the asset scan happens once before first use
    /// instead of on the first console command / drop / equip path.
    /// </summary>
    /// <remarks>
    /// When <see cref="verbose"/> is set, every registered key is printed via <see cref="Log.Debug"/>
    /// so you can confirm at boot that every <see cref="Trait"/> asset under <c>Resources/Traits/</c>
    /// was picked up.
    /// </remarks>
    [AddComponentMenu("Spellbound/Modifiers/Trait Registry Loader"), DefaultExecutionOrder(-1000)]
    public sealed class TraitRegistryLoader : MonoBehaviour {
        [SerializeField,
         Tooltip("When true, prints every discovered Trait key via Log.Debug after the scan.")]
        private bool verbose;

        private void Awake() {
            TraitRegistry.Refresh();

            if (!verbose)
                return;

            foreach (var key in TraitRegistry.Keys)
                Log.Debug($"[TraitRegistry] Registered: {key}");
        }
    }
}
