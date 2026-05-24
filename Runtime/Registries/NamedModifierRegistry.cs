// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Reflection;
using Spellbound.Core.Logging;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Reflects across loaded assemblies and indexes every concrete <see cref="SbModifier"/> subclass tagged
    /// with <see cref="NamedModifierAttribute"/> by its declared name. Duplicate names are logged as errors
    /// at discovery time so collisions surface immediately rather than at first use.
    /// </summary>
    /// <remarks>
    /// Lazy-loads on first query; the static cache is wiped naturally by Unity's domain reload on script
    /// recompile. The drop-in <c>NamedModifierRegistryLoader</c> can be added to a bootstrap scene to force
    /// eager load (and optional debug printing of discovered names). Modifiers must have a parameterless
    /// constructor — runtime instantiation is via <see cref="Activator.CreateInstance(Type)"/>.
    /// </remarks>
    public static class NamedModifierRegistry {
        private static Dictionary<string, Type> _byName;

        public static bool TryCreate(string name, out SbModifier modifier) {
            EnsureLoaded();
            modifier = null;

            if (!_byName.TryGetValue(name, out var type))
                return false;

            modifier = (SbModifier)Activator.CreateInstance(type);

            return true;
        }

        public static IEnumerable<string> Names {
            get {
                EnsureLoaded();

                return _byName.Keys;
            }
        }

        /// <summary>
        /// Force a rescan of loaded assemblies. The lazy path is normally enough; call this from a loader
        /// component to eagerly populate the cache before first use, or after dynamic assembly load.
        /// </summary>
        public static void Refresh() {
            _byName = null;
            EnsureLoaded();
        }

        private static void EnsureLoaded() {
            if (_byName != null)
                return;

            _byName = new Dictionary<string, Type>();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                Type[] types;

                try {
                    types = asm.GetTypes();
                }
                catch {
                    continue;
                }

                foreach (var type in types) {
                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    if (!typeof(SbModifier).IsAssignableFrom(type))
                        continue;

                    if (type.GetConstructor(Type.EmptyTypes) == null)
                        continue;

                    var attr = type.GetCustomAttribute<NamedModifierAttribute>(false);

                    if (attr == null)
                        continue;

                    if (_byName.TryGetValue(attr.Name, out var existing)) {
                        Log.Error(
                            $"[NamedModifierRegistry] Duplicate name '{attr.Name}'. " +
                            $"Existing: {existing.FullName}; ignored: {type.FullName}.");

                        continue;
                    }

                    _byName[attr.Name] = type;
                }
            }
        }
    }
}
