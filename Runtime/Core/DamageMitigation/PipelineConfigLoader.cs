// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Drop-in component that activates one or more <c>PipelineConfig&lt;TContext&gt;</c> assets at startup.
    /// Inspector-assigned configs take precedence; otherwise the loader scans every <c>Resources/</c> folder
    /// for assets derived from <see cref="ScriptableObject"/> that close <see cref="PipelineConfig{TContext}"/>.
    /// </summary>
    /// <remarks>
    /// Activating a config bakes its stage list into the hot-path array and sets the per-closed-generic
    /// <c>PipelineConfig&lt;TContext&gt;.Active</c> static — see <see cref="Pipeline.Run{TContext}"/>.
    /// </remarks>
    [AddComponentMenu("Spellbound/Modifiers/Pipeline Config Loader"), DefaultExecutionOrder(-1000)]
    public sealed class PipelineConfigLoader : MonoBehaviour {
        [SerializeField,
         Tooltip("Pipeline config assets to activate at startup. If empty, the loader scans every Resources/ " +
                 "folder for PipelineConfig assets and activates whatever it finds.")]
        private ScriptableObject[] configs;

        private void Awake() {
            var toActivate = new List<ScriptableObject>();

            if (configs != null) {
                foreach (var c in configs) {
                    if (c != null && IsPipelineConfig(c.GetType()))
                        toActivate.Add(c);
                }
            }

            if (toActivate.Count == 0) {
                var found = Resources.LoadAll<ScriptableObject>("");

                foreach (var c in found) {
                    if (c != null && IsPipelineConfig(c.GetType()))
                        toActivate.Add(c);
                }
            }

            if (toActivate.Count == 0) {
                Log.Error(
                    "[PipelineConfigLoader] No PipelineConfig assets found. Assign at least one config in the " +
                    "inspector or place one under a Resources/ folder.");

                return;
            }

            foreach (var config in toActivate)
                Activate(config);
        }

        private static bool IsPipelineConfig(Type t) {
            while (t != null && t != typeof(object)) {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(PipelineConfig<>))
                    return true;

                t = t.BaseType;
            }

            return false;
        }

        private static void Activate(ScriptableObject config) {
            var t = config.GetType();

            while (t != null && t != typeof(object)) {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(PipelineConfig<>)) {
                    t.GetMethod("Activate")?.Invoke(config, null);

                    return;
                }

                t = t.BaseType;
            }
        }
    }
}