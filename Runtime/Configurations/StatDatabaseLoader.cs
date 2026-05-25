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
        [SerializeField,
         Tooltip("StatDatabase asset to register at startup. " +
                 "If null, the first StatDatabase found in any Resources folder is used.")]
        private StatDatabase statDatabase;

        [SerializeField,
         Tooltip("When true, registering a stat that is not declared in the database throws at runtime. " +
                 "Recommended for shipping builds.")]
        private bool strictStatValidation = true;

        public StatDatabase Database => statDatabase;

        private void Awake() {
            if (statDatabase == null) {
                var found = Resources.LoadAll<StatDatabase>("");

                if (found.Length > 0)
                    statDatabase = found[0];
            }

            if (statDatabase == null) {
                Log.Error(
                    "[StatDatabaseLoader] StatDatabase is null and none was found in any Resources folder. " +
                    "Assign a database in the inspector or place one under Resources/.");

                return;
            }

            statDatabase.RegisterAll(strictStatValidation);
        }
    }
}