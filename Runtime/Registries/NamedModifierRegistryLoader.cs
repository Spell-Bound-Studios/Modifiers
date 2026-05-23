// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Drop-in component that eagerly initializes <see cref="NamedModifierRegistry"/> at startup. Add it to
    /// any GameObject in your bootstrap scene so the reflection scan happens once before first use instead
    /// of on the first console command / talent application.
    /// </summary>
    /// <remarks>
    /// When <see cref="verbose"/> is set, every registered name is printed via <see cref="Log.Debug"/> so
    /// you can confirm at boot that every <see cref="NamedModifierAttribute"/>-tagged type was picked up.
    /// </remarks>
    [AddComponentMenu("Spellbound/Modifiers/Named Modifier Registry Loader"), DefaultExecutionOrder(-1000)]
    public sealed class NamedModifierRegistryLoader : MonoBehaviour {
        [SerializeField,
         Tooltip("When true, prints every discovered NamedModifier name via Log.Debug after the scan.")]
        private bool verbose;

        private void Awake() {
            NamedModifierRegistry.Refresh();

            if (!verbose)
                return;

            foreach (var mod in NamedModifierRegistry.Names)
                Log.Debug($"[NamedModifierRegistry] Registered: {mod}");
        }
    }
}
