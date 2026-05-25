// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Drop-in component that registers a <see cref="StatDatabase"/> with the stat system at startup.
    /// Add it to any GameObject in your bootstrap scene to initialize stats without writing code.
    /// </summary>
    /// <remarks>
    /// Resolution order during Awake:
    /// 1. The inspector-assigned <see cref="StatDatabase"/> reference, if any.
    /// 2. The first <see cref="StatDatabase"/> found in any Resources folder.
    /// 3. If both fail, logs an error and does nothing.
    /// </remarks>
    [AddComponentMenu("Spellbound/Modifiers/Stat Database Loader"), DefaultExecutionOrder(-1000)]
    public sealed class StatDatabaseLoader : MonoBehaviour {
        [SerializeField] private StatDatabase statDatabase;
        [SerializeField] private bool strictStatValidation = true;

        public StatDatabase Database => statDatabase;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EarlyInit() {
            var found = Resources.LoadAll<StatDatabase>("");
            if (found.Length > 0)
                found[0].RegisterAll();
        }

        private void Awake() {
            // Full init with inspector-assigned database + strict validation
            if (statDatabase == null) {
                var found = Resources.LoadAll<StatDatabase>("");
                if (found.Length > 0)
                    statDatabase = found[0];
            }

            if (statDatabase == null) {
                Log.Error("[StatDatabaseLoader] StatDatabase not found.");
                return;
            }

            statDatabase.RegisterAll(strictStatValidation);
        }
    }
}