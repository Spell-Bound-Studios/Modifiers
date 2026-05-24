// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Editor {
    /// <summary>
    /// Small reusable building blocks for the lib's editor drawers / inspectors: serialized-array mutation
    /// helpers, visible-child iteration, and the two repeated UI Toolkit elements (icon button, section
    /// header). Lives in the Editor assembly so every drawer in this lib can share one source of truth.
    /// </summary>
    internal static class EditorListHelpers {
        // ============================================================================================
        // Serialized array mutations
        // ============================================================================================

        /// <summary>
        /// Move <paramref name="index"/> one slot up; no-op (returns false) if already at the top. Applies
        /// modified properties on success so the caller only handles its own onChanged refresh.
        /// </summary>
        public static bool MoveUp(SerializedProperty list, int index) {
            if (index <= 0)
                return false;

            list.MoveArrayElement(index, index - 1);
            list.serializedObject.ApplyModifiedProperties();

            return true;
        }

        /// <summary>
        /// Move <paramref name="index"/> one slot down; no-op (returns false) if already at the bottom.
        /// </summary>
        public static bool MoveDown(SerializedProperty list, int index) {
            if (index >= list.arraySize - 1)
                return false;

            list.MoveArrayElement(index, index + 1);
            list.serializedObject.ApplyModifiedProperties();

            return true;
        }

        /// <summary>
        /// Remove the element at <paramref name="index"/>. For <c>[SerializeReference]</c> arrays, clears the
        /// managed reference first so the slot doesn't leak; for value arrays the clear is a no-op.
        /// </summary>
        public static void RemoveAt(SerializedProperty list, int index) {
            var element = list.GetArrayElementAtIndex(index);

            if (element.propertyType == SerializedPropertyType.ManagedReference)
                element.managedReferenceValue = null;

            list.DeleteArrayElementAtIndex(index);
            list.serializedObject.ApplyModifiedProperties();
        }

        // ============================================================================================
        // Iteration
        // ============================================================================================

        /// <summary>
        /// Invoke <paramref name="visit"/> for each direct visible child of <paramref name="property"/>,
        /// stopping at the end-property sentinel. The same iterator is passed to every call, so the visitor
        /// must <c>Copy()</c> if it needs to hold the reference past its own scope (e.g. for a PropertyField).
        /// </summary>
        public static void ForEachVisibleChild(SerializedProperty property, Action<SerializedProperty> visit) {
            var iterator = property.Copy();
            var end = property.GetEndProperty();

            if (!iterator.NextVisible(true))
                return;

            do {
                if (SerializedProperty.EqualContents(iterator, end))
                    break;

                visit(iterator);
            } while (iterator.NextVisible(false));
        }

        // ============================================================================================
        // Shared UI Toolkit elements
        // ============================================================================================

        /// <summary>
        /// Compact 22-wide button used for ▲ / ▼ / ✕ row actions in inline lists.
        /// </summary>
        public static Button IconButton(string text, Action onClick) =>
                new(onClick) {
                    text = text,
                    style = {
                        width = 22,
                        marginLeft = 2,
                        paddingLeft = 2,
                        paddingRight = 2
                    }
                };

        /// <summary>
        /// Subtle section header label used between authored regions of a custom drawer.
        /// </summary>
        public static VisualElement SectionHeader(string text) =>
                new Label(text) {
                    style = {
                        unityFontStyleAndWeight = FontStyle.Bold,
                        fontSize = 12,
                        color = new Color(0.78f, 0.78f, 0.78f),
                        marginTop = 8,
                        marginBottom = 2
                    }
                };

        /// <summary>
        /// Walk <paramref name="parent"/>'s visible children and return the first one that's a generic
        /// array (i.e. a <c>List&lt;T&gt;</c> or <c>T[]</c>). Used by container drawers to locate their
        /// inner serialized list by SHAPE rather than by field name — so the container's list field can be
        /// renamed without breaking the drawer.
        /// </summary>
        public static SerializedProperty FindFirstGenericArrayChild(SerializedProperty parent) {
            SerializedProperty found = null;

            ForEachVisibleChild(parent, child => {
                if (found != null)
                    return;

                if (child.isArray && child.propertyType == SerializedPropertyType.Generic)
                    found = child.Copy();
            });

            return found;
        }

        // ============================================================================================
        // Type discovery
        // ============================================================================================

        /// <summary>
        /// Walk every loaded assembly and return concrete, serializable types assignable to
        /// <paramref name="baseType"/>. Optionally filter by a required class-level attribute — useful for
        /// opt-in picker lists (e.g. only types tagged <see cref="PickableBehaviourAttribute"/>).
        /// </summary>
        public static List<Type> GetAssignableTypes(Type baseType, Type requiredAttribute = null) {
            var types = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                try {
                    foreach (var type in assembly.GetTypes()) {
                        if (type.IsAbstract || type.IsInterface)
                            continue;

                        if (!baseType.IsAssignableFrom(type))
                            continue;

                        if (type.GetConstructor(Type.EmptyTypes) == null)
                            continue;

                        if (!type.IsSerializable &&
                            type.GetCustomAttributes(typeof(SerializableAttribute), true).Length == 0)
                            continue;

                        if (requiredAttribute != null &&
                            type.GetCustomAttributes(requiredAttribute, true).Length == 0)
                            continue;

                        types.Add(type);
                    }
                }
                catch {
                    // Skip problematic assemblies
                }
            }

            return types.OrderBy(t => t.Name).ToList();
        }

        /// <summary>
        /// Styled read-only preview box that re-runs <paramref name="compute"/> whenever any field on
        /// <paramref name="so"/> changes. Caller supplies the text producer; the box, label, and tracking
        /// hookup are shared so every drawer's "live preview" looks the same.
        /// </summary>
        public static VisualElement BuildLivePreview(SerializedObject so, Func<string> compute) {
            var container = new VisualElement {
                style = {
                    marginLeft = 4,
                    marginBottom = 6,
                    paddingTop = 6,
                    paddingBottom = 6,
                    paddingLeft = 10,
                    paddingRight = 10,
                    backgroundColor = new Color(0f, 0f, 0f, 0.12f),
                    borderLeftWidth = 2,
                    borderLeftColor = new Color(0.7f, 0.7f, 0.4f)
                }
            };

            var output = new Label("(computing…)") {
                style = {
                    whiteSpace = WhiteSpace.Normal,
                    fontSize = 12
                }
            };

            container.Add(output);

            container.TrackSerializedObjectValue(so, _ => Refresh());
            Refresh();

            return container;

            void Refresh() => output.text = compute();
        }
    }
}
