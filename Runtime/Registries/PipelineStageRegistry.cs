// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Reflects over loaded assemblies once and exposes every concrete type tagged with
    /// <see cref="PipelineStageAttribute"/>, grouped by their declared context type. Editor tools query this
    /// to populate type pickers for each <see cref="PipelineTemplate{TContext}"/>.
    /// </summary>
    public static class PipelineStageRegistry {
        public readonly struct StageEntry {
            public readonly Type Type;
            public readonly Type ContextType;
            public readonly string DisplayName;
            public readonly string Description;

            public StageEntry(Type type, Type contextType, string displayName, string description) {
                Type = type;
                ContextType = contextType;
                DisplayName = displayName;
                Description = description;
            }
        }

        private static StageEntry[] _all;

        public static IReadOnlyList<StageEntry> All {
            get {
                if (_all == null)
                    Refresh();

                return _all;
            }
        }

        public static IEnumerable<StageEntry> GetStagesForContext(Type contextType) {
            foreach (var entry in All) {
                if (entry.ContextType == contextType)
                    yield return entry;
            }
        }

        public static void Refresh() {
            var list = new List<StageEntry>();

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

                    var attr = type.GetCustomAttribute<PipelineStageAttribute>(false);

                    if (attr == null)
                        continue;

                    if (attr.ContextType == null)
                        continue;

                    var expectedInterface = typeof(IPipelineStage<>).MakeGenericType(attr.ContextType);

                    if (!expectedInterface.IsAssignableFrom(type))
                        continue;

                    if (type.GetConstructor(Type.EmptyTypes) == null)
                        continue;

                    list.Add(new StageEntry(
                        type,
                        attr.ContextType,
                        string.IsNullOrEmpty(attr.DisplayName) ? type.Name : attr.DisplayName,
                        attr.Description));
                }
            }

            _all = list.OrderBy(e => e.DisplayName).ToArray();
        }
    }
}