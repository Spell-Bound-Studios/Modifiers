// Copyright 2026 Spellbound Studio Inc.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Spellbound.Core.Modules;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Modifiers.Editor {
    /// <summary>
    /// Single UI Toolkit property drawer for every <see cref="PresetModule"/> subclass — current and future.
    /// Behaviour:
    /// <list type="bullet">
    /// <item>For each serialized field on the module: if the field is a <c>List&lt;T&gt;</c> (or <c>T[]</c>)
    /// where <c>T</c> carries <see cref="InlineTemplateAttribute"/>, render a compact list with one row per
    /// element. Otherwise, render via Unity's default <c>PropertyField</c>, which honors <c>[Header]</c>,
    /// <c>[Tooltip]</c>, and any per-field custom drawers.</item>
    /// <item>After all fields, if the module declares any stat-relevant template lists
    /// (<see cref="StatTemplate"/>, <see cref="StatModifierTemplate"/>), append a read-only computed-stats
    /// preview that updates whenever any template value changes.</item>
    /// </list>
    /// New module types and new template structs both work without touching this file — declare fields,
    /// mark your template with <see cref="InlineTemplateAttribute"/>, done.
    /// </summary>
    [CustomPropertyDrawer(typeof(PresetModule), true)]
    public sealed class PresetModuleDrawer : PropertyDrawer {
        private const BindingFlags FieldFlags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var root = new VisualElement {
                style = {
                    marginTop = 2
                }
            };

            // Track which stat-relevant template lists we saw so we can attach the preview at the end.
            SerializedProperty resourceTemplatesProp = null;
            SerializedProperty statTemplatesProp = null;
            SerializedProperty statModifierTemplatesProp = null;

            var moduleInstance = property.managedReferenceValue;
            var moduleType = moduleInstance?.GetType();

            var iterator = property.Copy();
            var end = property.GetEndProperty();

            if (iterator.NextVisible(true)) {
                while (!SerializedProperty.EqualContents(iterator, end)) {
                    var current = iterator.Copy();
                    var info = TryGetInlineTemplateInfo(moduleType, current);

                    if (info.ElementType != null) {
                        // One header per template list: [Header(...)] when present on the field, otherwise the
                        // nicified field name. The list itself renders bare so we never double-header.
                        var headerAttr = info.Field?.GetCustomAttribute<HeaderAttribute>(false);

                        var headerText = headerAttr != null && !string.IsNullOrEmpty(headerAttr.header)
                                ? headerAttr.header
                                : ObjectNames.NicifyVariableName(current.displayName);

                        root.Add(SectionHeader(headerText));
                        root.Add(BuildTemplateList(current, info.ElementType));

                        if (info.ElementType == typeof(StatTemplate))
                            statTemplatesProp = current;
                        else if (info.ElementType == typeof(StatModifierTemplate))
                            statModifierTemplatesProp = current;
                        else if (info.ElementType == typeof(ResourceTemplate))
                            resourceTemplatesProp = current;
                    }
                    else {
                        var pf = new PropertyField(current);
                        pf.Bind(property.serializedObject);
                        root.Add(pf);
                    }

                    if (!iterator.NextVisible(false))
                        break;
                }
            }

            // Computed-stats preview is automatic when stat-relevant templates are present.
            if (resourceTemplatesProp == null && statTemplatesProp == null &&
                statModifierTemplatesProp == null) return root;

            root.Add(SectionHeader("Computed Stats (read-only preview)"));
            root.Add(BuildComputedPreview(resourceTemplatesProp, statTemplatesProp, statModifierTemplatesProp));

            return root;
        }

        // ============================================================================================
        // Field-type detection
        // ============================================================================================

        private readonly struct InlineTemplateInfo {
            public readonly Type ElementType;
            public readonly FieldInfo Field;

            public InlineTemplateInfo(Type elementType, FieldInfo field) {
                ElementType = elementType;
                Field = field;
            }
        }

        /// <summary>
        /// If <paramref name="listProp"/> is a serialized list/array whose element type is a struct tagged with
        /// <see cref="InlineTemplateAttribute"/>, returns the element type and the resolved <see cref="FieldInfo"/>
        /// (so callers can read <c>[Header]</c>/<c>[Tooltip]</c>/etc.). Otherwise the returned struct is default.
        /// </summary>
        private static InlineTemplateInfo TryGetInlineTemplateInfo(Type moduleType, SerializedProperty listProp) {
            if (!listProp.isArray)
                return default;

            // Strings are technically arrays under the hood; explicitly exclude.
            if (listProp.propertyType == SerializedPropertyType.String)
                return default;

            if (moduleType == null)
                return default;

            // Walk the module's inheritance chain to find the FieldInfo for this serialized field.
            FieldInfo field = null;
            var t = moduleType;

            while (t != null && t != typeof(object) && field == null) {
                field = t.GetField(listProp.name, FieldFlags);
                t = t.BaseType;
            }

            if (field == null)
                return default;

            var fieldType = field.FieldType;

            Type elementType = null;

            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                elementType = fieldType.GetGenericArguments()[0];
            else if (fieldType.IsArray)
                elementType = fieldType.GetElementType();

            if (elementType == null)
                return default;

            return elementType.GetCustomAttribute<InlineTemplateAttribute>(false) != null
                    ? new InlineTemplateInfo(elementType, field)
                    : default;
        }

        // ============================================================================================
        // Template list rendering
        // ============================================================================================

        private static VisualElement BuildTemplateList(SerializedProperty listProp, Type elementType) {
            var container = new VisualElement {
                style = {
                    marginTop = 2,
                    marginBottom = 4,
                    paddingLeft = 6
                }
            };

            var itemsContainer = new VisualElement();
            container.Add(itemsContainer);

            void Refresh() {
                listProp.serializedObject.Update();
                itemsContainer.Clear();

                for (var i = 0; i < listProp.arraySize; i++)
                    itemsContainer.Add(BuildListRow(listProp, i, Refresh));
            }

            Refresh();

            var addBtn = new Button(() => {
                listProp.arraySize++;
                listProp.serializedObject.ApplyModifiedProperties();
                Refresh();
            }) {
                text = $"+ Add {ObjectNames.NicifyVariableName(elementType.Name)}",
                style = {
                    marginTop = 2,
                    alignSelf = Align.FlexStart,
                    paddingLeft = 8,
                    paddingRight = 8
                }
            };

            container.Add(addBtn);

            return container;
        }

        private static VisualElement BuildListRow(SerializedProperty listProp, int index, Action onChanged) {
            var capturedIndex = index;
            var elementProp = listProp.GetArrayElementAtIndex(index);

            var row = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 2
                }
            };

            var indexLabel = new Label($"{index + 1}.") {
                style = {
                    minWidth = 22,
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };

            row.Add(indexLabel);

            var fieldArea = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    flexGrow = 1
                }
            };

            FillInlineFields(fieldArea, elementProp);
            row.Add(fieldArea);

            row.Add(IconButton("▲", () => {
                if (capturedIndex == 0)
                    return;

                listProp.MoveArrayElement(capturedIndex, capturedIndex - 1);
                listProp.serializedObject.ApplyModifiedProperties();
                onChanged();
            }));

            row.Add(IconButton("▼", () => {
                if (capturedIndex >= listProp.arraySize - 1)
                    return;

                listProp.MoveArrayElement(capturedIndex, capturedIndex + 1);
                listProp.serializedObject.ApplyModifiedProperties();
                onChanged();
            }));

            row.Add(IconButton("✕", () => {
                listProp.DeleteArrayElementAtIndex(capturedIndex);
                listProp.serializedObject.ApplyModifiedProperties();
                onChanged();
            }));

            return row;
        }

        /// <summary>
        /// Render each child field of the element struct inline (horizontally). The first field (typically a
        /// <c>StatDefinition</c> object reference) renders unlabelled — the dropdown itself names the entry,
        /// so a "Definition:" prefix would be pure noise. Every subsequent field gets a compact inline label
        /// so designers can tell which 0 is base-value vs. min. Works for any struct shape — no per-template
        /// hardcoding required.
        /// </summary>
        private static void FillInlineFields(VisualElement rowContent, SerializedProperty elementProp) {
            var iterator = elementProp.Copy();
            var end = elementProp.GetEndProperty();

            if (!iterator.NextVisible(true))
                return;

            var isFirst = true;

            do {
                if (SerializedProperty.EqualContents(iterator, end))
                    break;

                var cell = new VisualElement {
                    style = {
                        flexGrow = 1,
                        marginRight = 4,
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center
                    }
                };

                if (!isFirst) {
                    cell.Add(new Label(iterator.displayName) {
                        style = {
                            marginRight = 4,
                            color = new Color(0.75f, 0.75f, 0.75f),
                            unityTextAlign = TextAnchor.MiddleLeft,
                            minWidth = 0,
                            paddingLeft = 0,
                            paddingRight = 0
                        }
                    });
                }

                var pf = new PropertyField(iterator.Copy(), string.Empty) {
                    style = { flexGrow = 1 }
                };

                pf.Bind(elementProp.serializedObject);
                cell.Add(pf);
                rowContent.Add(cell);

                isFirst = false;
            } while (iterator.NextVisible(false));
        }

        // ============================================================================================
        // Computed-stats preview
        // ============================================================================================

        private static VisualElement BuildComputedPreview(
            SerializedProperty resources,
            SerializedProperty stats,
            SerializedProperty modifiers) {
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

            // The preview reads from the same serialized object the property belongs to. Listening on that
            // object catches changes to any template field — TrackSerializedObjectValue fires on edits.
            var serializedObject = (resources ?? stats ?? modifiers).serializedObject;

            void Refresh() => output.text = ComputePreviewText(resources, stats, modifiers);

            container.TrackSerializedObjectValue(serializedObject, _ => Refresh());
            Refresh();

            return container;
        }

        private static string ComputePreviewText(
            SerializedProperty resources,
            SerializedProperty stats,
            SerializedProperty modifiers) {
            try {
                var container = new StatContainer();
                var resourceIds = new List<int>();
                var resourceMins = new Dictionary<int, float>();
                var statIds = new HashSet<int>();

                if (resources != null) {
                    for (var i = 0; i < resources.arraySize; i++) {
                        var entry = resources.GetArrayElementAtIndex(i);
                        var def = entry.FindPropertyRelative("definition").objectReferenceValue as StatDefinition;

                        if (def == null || string.IsNullOrEmpty(def.StatName))
                            continue;

                        var id = StatRegistry.Register(def.StatName);
                        container.SetBase(id, entry.FindPropertyRelative("baseValue").floatValue);
                        resourceMins[id] = entry.FindPropertyRelative("min").floatValue;

                        if (!resourceIds.Contains(id))
                            resourceIds.Add(id);
                    }
                }

                if (stats != null) {
                    for (var i = 0; i < stats.arraySize; i++) {
                        var entry = stats.GetArrayElementAtIndex(i);
                        var def = entry.FindPropertyRelative("definition").objectReferenceValue as StatDefinition;

                        if (def == null || string.IsNullOrEmpty(def.StatName))
                            continue;

                        var id = StatRegistry.Register(def.StatName);
                        container.SetBase(id, entry.FindPropertyRelative("baseValue").floatValue);
                        statIds.Add(id);
                    }
                }

                if (modifiers != null) {
                    for (var i = 0; i < modifiers.arraySize; i++) {
                        var entry = modifiers.GetArrayElementAtIndex(i);
                        var def = entry.FindPropertyRelative("definition").objectReferenceValue as StatDefinition;

                        if (def == null || string.IsNullOrEmpty(def.StatName))
                            continue;

                        var id = StatRegistry.Register(def.StatName);
                        var type = (ModifierType)entry.FindPropertyRelative("type").enumValueIndex;
                        var value = entry.FindPropertyRelative("value").floatValue;
                        container.AddModifier(new StatModifier(id, type, value));

                        // A modifier that targets a resource's backing stat moves the resource's max, not a
                        // separate stat — don't list it twice.
                        if (!resourceIds.Contains(id))
                            statIds.Add(id);
                    }
                }

                var sb = new StringBuilder();

                if (resourceIds.Count == 0 && statIds.Count == 0) {
                    sb.Append("(no stats declared — add templates above to see computed values)");

                    return sb.ToString().TrimEnd();
                }

                if (resourceIds.Count > 0) {
                    sb.Append("Resources:\n");

                    foreach (var id in resourceIds) {
                        var name = StatRegistry.GetName(id);
                        var max = container.GetValue(id);
                        var min = resourceMins.GetValueOrDefault(id, 0f);

                        sb.Append("  ").Append(name)
                                .Append(": max ").Append(max.ToString("F2"))
                                .Append(", min ").Append(min.ToString("F2"))
                                .Append('\n');
                    }
                }

                if (statIds.Count <= 0)
                    return sb.ToString().TrimEnd();

                {
                    if (resourceIds.Count > 0)
                        sb.Append('\n');

                    sb.Append("Stats:\n");

                    foreach (var id in statIds) {
                        var name = StatRegistry.GetName(id);
                        var baseVal = container.GetBase(id);
                        var finalVal = container.GetValue(id);
                        sb.Append("  ").Append(name).Append(": ").Append(finalVal.ToString("F2"));

                        if (Mathf.Abs(finalVal - baseVal) > 0.0001f)
                            sb.Append("  (base ").Append(baseVal.ToString("F2")).Append(")");

                        sb.Append('\n');
                    }
                }

                return sb.ToString().TrimEnd();
            }
            catch (Exception ex) {
                return $"<preview error: {ex.Message}>";
            }
        }

        // ============================================================================================
        // Small UI helpers
        // ============================================================================================

        private static VisualElement SectionHeader(string text) =>
                new Label(text) {
                    style = {
                        unityFontStyleAndWeight = FontStyle.Bold,
                        fontSize = 12,
                        color = new Color(0.78f, 0.78f, 0.78f),
                        marginTop = 8,
                        marginBottom = 2
                    }
                };

        private static Button IconButton(string text, Action onClick) =>
                new(onClick) {
                    text = text,
                    style = {
                        width = 22,
                        marginLeft = 2,
                        paddingLeft = 2,
                        paddingRight = 2
                    }
                };
    }
}
#endif