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
    /// Property drawer powering <see cref="DropdownPickerAttribute"/>. Handles three field shapes:
    /// <c>[SerializeReference]</c> (lists every concrete serializable implementer), <c>ObjectReference</c>
    /// (lists every matching ScriptableObject asset in the project), and <c>List&lt;T&gt;</c> / <c>T[]</c> of
    /// either kind (renders each element with add / remove buttons). Ships both UI Toolkit and IMGUI paths
    /// so it works inside ObjectPreset inspectors that still render via OnInspectorGUI.
    /// </summary>
    [CustomPropertyDrawer(typeof(DropdownPickerAttribute))]
    public class DropdownPickerDrawer : PropertyDrawer {
        // ============================================================================================
        // UI TOOLKIT PATH — used when the host inspector renders via CreateInspectorGUI / UI Toolkit.
        // ============================================================================================

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            if (property.isArray && property.propertyType == SerializedPropertyType.Generic)
                return CreateListPicker_UITK(property);

            return property.propertyType switch {
                SerializedPropertyType.ManagedReference => CreateSerializeReferencePicker_UITK(property),
                SerializedPropertyType.ObjectReference => CreateAssetPicker_UITK(property),
                _ => new Label($"[DropdownPicker] Unsupported property type: {property.propertyType}") {
                    style = { color = Color.red }
                }
            };
        }

        private VisualElement CreateListPicker_UITK(SerializedProperty listProperty) {
            var container = new VisualElement();
            container.style.marginBottom = 4;

            var header = new Label(listProperty.displayName) {
                style = {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 2
                }
            };

            container.Add(header);

            var itemsContainer = new VisualElement {
                style = {
                    marginLeft = 8
                }
            };
            container.Add(itemsContainer);

            Refresh();

            var addBtn = new Button(() => {
                listProperty.arraySize++;
                var newElement = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);

                if (newElement.propertyType == SerializedPropertyType.ManagedReference)
                    newElement.managedReferenceValue = null;

                listProperty.serializedObject.ApplyModifiedProperties();
                Refresh();
            }) {
                text = "+ Add Element",
                style = { marginTop = 2 }
            };

            container.Add(addBtn);

            return container;

            void Refresh() {
                listProperty.serializedObject.Update();
                itemsContainer.Clear();

                for (var i = 0; i < listProperty.arraySize; i++) {
                    var capturedIndex = i;
                    var element = listProperty.GetArrayElementAtIndex(i);

                    var row = new VisualElement {
                        style = {
                            flexDirection = FlexDirection.Row,
                            alignItems = Align.FlexStart,
                            marginBottom = 4,
                            borderLeftWidth = 2,
                            borderLeftColor = new Color(0.4f, 0.4f, 0.4f),
                            paddingLeft = 4
                        }
                    };

                    var indexLabel = new Label($"[{i}]") { style = { minWidth = 24 } };

                    var elementUi = new VisualElement {
                        style = {
                            flexGrow = 1
                        }
                    };

                    switch (element.propertyType) {
                        case SerializedPropertyType.ManagedReference:
                            elementUi.Add(CreateSerializeReferencePicker_UITK(element));

                            break;
                        case SerializedPropertyType.ObjectReference:
                            elementUi.Add(CreateAssetPicker_UITK(element));

                            break;
                        case SerializedPropertyType.Generic:
                        case SerializedPropertyType.Integer:
                        case SerializedPropertyType.Boolean:
                        case SerializedPropertyType.Float:
                        case SerializedPropertyType.String:
                        case SerializedPropertyType.Color:
                        case SerializedPropertyType.LayerMask:
                        case SerializedPropertyType.Enum:
                        case SerializedPropertyType.Vector2:
                        case SerializedPropertyType.Vector3:
                        case SerializedPropertyType.Vector4:
                        case SerializedPropertyType.Rect:
                        case SerializedPropertyType.ArraySize:
                        case SerializedPropertyType.Character:
                        case SerializedPropertyType.AnimationCurve:
                        case SerializedPropertyType.Bounds:
                        case SerializedPropertyType.Gradient:
                        case SerializedPropertyType.Quaternion:
                        case SerializedPropertyType.ExposedReference:
                        case SerializedPropertyType.FixedBufferSize:
                        case SerializedPropertyType.Vector2Int:
                        case SerializedPropertyType.Vector3Int:
                        case SerializedPropertyType.RectInt:
                        case SerializedPropertyType.BoundsInt:
                        case SerializedPropertyType.Hash128:
                        case SerializedPropertyType.RenderingLayerMask:
                        default:
                            elementUi.Add(new PropertyField(element));

                            break;
                    }

                    var removeBtn = new Button(() => {
                        EditorListHelpers.RemoveAt(listProperty, capturedIndex);
                        Refresh();
                    }) {
                        text = "✕",
                        style = { width = 22, marginLeft = 4 }
                    };

                    row.Add(indexLabel);
                    row.Add(elementUi);
                    row.Add(removeBtn);
                    itemsContainer.Add(row);
                }
            }
        }

        private VisualElement CreateSerializeReferencePicker_UITK(SerializedProperty property) {
            var container = new VisualElement();

            var fieldType = GetManagedReferenceFieldType(property);

            if (fieldType == null) {
                container.Add(new Label($"Could not determine field type for {property.propertyPath}"));

                return container;
            }

            var availableTypes = GetAssignableTypes(fieldType);

            var typeNames = new List<string> { "(None)" };
            typeNames.AddRange(availableTypes.Select(t => t.Name));

            var currentTypeName = property.managedReferenceValue?.GetType().Name ?? "(None)";
            var currentIndex = typeNames.IndexOf(currentTypeName);
            if (currentIndex < 0) currentIndex = 0;

            var row = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 2
                }
            };

            var label = new Label(property.displayName) {
                style = {
                    minWidth = 150
                }
            };

            var dropdown = new DropdownField(typeNames, currentIndex) {
                style = {
                    flexGrow = 1
                }
            };

            dropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == "(None)")
                    property.managedReferenceValue = null;
                else {
                    var selectedType = availableTypes.FirstOrDefault(t => t.Name == evt.newValue);

                    if (selectedType != null)
                        property.managedReferenceValue = Activator.CreateInstance(selectedType);
                }

                property.serializedObject.ApplyModifiedProperties();
            });

            row.Add(label);
            row.Add(dropdown);
            container.Add(row);

            if (property.managedReferenceValue != null) {
                if (IsListElement(property)) {
                    // List-element context: the element property does NOT carry the [DropdownPicker]
                    // attribute (that's on the parent list), so a PropertyField here resolves drawers by
                    // the element's declared type — e.g. SbBehaviourDrawer fires for SbBehaviour
                    // subclasses, giving us the typed fields + Stats preview panel.
                    var field = new PropertyField(property);
                    field.Bind(property.serializedObject);
                    container.Add(field);
                }
                else {
                    // Top-level context: the property itself carries [DropdownPicker]; calling
                    // PropertyField here would re-fire this same drawer recursively. Fall back to children
                    // iteration so default per-field rendering happens without re-entering.
                    var foldout = new Foldout { text = "Properties", value = true };

                    EditorListHelpers.ForEachVisibleChild(property, child => {
                        var field = new PropertyField(child.Copy());
                        field.Bind(property.serializedObject);
                        foldout.Add(field);
                    });

                    if (foldout.childCount > 0) container.Add(foldout);
                }
            }

            return container;
        }

        private VisualElement CreateAssetPicker_UITK(SerializedProperty property) {
            var container = new VisualElement();
            var fieldType = ResolveAssetFieldType();
            var assets = FindAssetsOfType(fieldType);

            var assetNames = new List<string> { "(None)" };
            assetNames.AddRange(assets.Select(a => a.name));

            var currentIndex = 0;
            var currentAsset = property.objectReferenceValue;

            if (currentAsset != null) {
                var index = assets.FindIndex(a => a == currentAsset);

                if (index >= 0)
                    currentIndex = index + 1;
            }

            var row = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 2
                }
            };

            var label = new Label(property.displayName) { style = { minWidth = 150 } };

            var dropdown = new DropdownField(assetNames, currentIndex) {
                style = {
                    flexGrow = 1
                }
            };

            dropdown.RegisterValueChangedCallback(evt => {
                property.objectReferenceValue =
                        evt.newValue == "(None)"
                                ? null
                                : assets.FirstOrDefault(a => a.name == evt.newValue);

                property.serializedObject.ApplyModifiedProperties();
            });

            row.Add(label);
            row.Add(dropdown);
            container.Add(row);

            return container;
        }

        // ============================================================================================
        // IMGUI PATH — used when the host inspector renders via OnInspectorGUI (the case for
        // ObjectPresetEditor today). Without this, Unity's default PropertyDrawer.OnGUI writes
        // "No GUI Implemented" into the field.
        // ============================================================================================

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (property.isArray && property.propertyType == SerializedPropertyType.Generic)
                return GetListHeight_IMGUI(property);

            if (property.propertyType == SerializedPropertyType.ManagedReference)
                return GetManagedReferenceHeight_IMGUI(property);

            if (property.propertyType == SerializedPropertyType.ObjectReference)
                return EditorGUIUtility.singleLineHeight;

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.isArray && property.propertyType == SerializedPropertyType.Generic) {
                DrawList_IMGUI(position, property, label);

                return;
            }

            if (property.propertyType == SerializedPropertyType.ManagedReference) {
                DrawManagedReference_IMGUI(position, property, label);

                return;
            }

            if (property.propertyType == SerializedPropertyType.ObjectReference) {
                DrawAsset_IMGUI(position, property, label);

                return;
            }

            EditorGUI.LabelField(position, label, new GUIContent($"Unsupported: {property.propertyType}"));
        }

        private float GetListHeight_IMGUI(SerializedProperty listProperty) {
            var h = EditorGUIUtility.singleLineHeight; // header

            for (var i = 0; i < listProperty.arraySize; i++) {
                var element = listProperty.GetArrayElementAtIndex(i);
                h += EditorGUIUtility.standardVerticalSpacing;

                if (element.propertyType == SerializedPropertyType.ManagedReference)
                    h += GetManagedReferenceHeight_IMGUI(element);
                else if (element.propertyType == SerializedPropertyType.ObjectReference)
                    h += EditorGUIUtility.singleLineHeight;
                else
                    h += EditorGUI.GetPropertyHeight(element, true);
            }

            h += EditorGUIUtility.standardVerticalSpacing;
            h += EditorGUIUtility.singleLineHeight; // add button

            return h;
        }

        private float GetManagedReferenceHeight_IMGUI(SerializedProperty property) {
            var h = EditorGUIUtility.singleLineHeight; // type dropdown

            if (property.managedReferenceValue == null)
                return h;

            EditorListHelpers.ForEachVisibleChild(property, child => {
                h += EditorGUIUtility.standardVerticalSpacing;
                h += EditorGUI.GetPropertyHeight(child, true);
            });

            return h;
        }

        private void DrawList_IMGUI(Rect position, SerializedProperty listProperty, GUIContent label) {
            var y = position.y;
            var lineH = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.LabelField(new Rect(position.x, y, position.width, lineH), label, EditorStyles.boldLabel);
            y += lineH;

            for (var i = 0; i < listProperty.arraySize; i++) {
                var element = listProperty.GetArrayElementAtIndex(i);
                y += spacing;

                float elementH;

                if (element.propertyType == SerializedPropertyType.ManagedReference)
                    elementH = GetManagedReferenceHeight_IMGUI(element);
                else if (element.propertyType == SerializedPropertyType.ObjectReference)
                    elementH = lineH;
                else
                    elementH = EditorGUI.GetPropertyHeight(element, true);

                const float removeW = 24f;
                const float indexW = 32f;
                var contentX = position.x + indexW;
                var contentW = position.width - indexW - removeW - 4f;
                var removeX = position.x + position.width - removeW;

                EditorGUI.LabelField(new Rect(position.x, y, indexW, lineH), $"[{i}]");

                var elementRect = new Rect(contentX, y, contentW, elementH);

                if (element.propertyType == SerializedPropertyType.ManagedReference)
                    DrawManagedReference_IMGUI(elementRect, element, new GUIContent($"Element {i}"));
                else if (element.propertyType == SerializedPropertyType.ObjectReference)
                    DrawAsset_IMGUI(elementRect, element, new GUIContent($"Element {i}"));
                else
                    EditorGUI.PropertyField(elementRect, element, new GUIContent($"Element {i}"), true);

                if (GUI.Button(new Rect(removeX, y, removeW, lineH), "✕")) {
                    EditorListHelpers.RemoveAt(listProperty, i);
                    GUIUtility.ExitGUI();

                    return;
                }

                y += elementH;
            }

            y += spacing;

            if (GUI.Button(new Rect(position.x, y, position.width, lineH), "+ Add Element")) {
                listProperty.arraySize++;
                var newElement = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);

                if (newElement.propertyType == SerializedPropertyType.ManagedReference)
                    newElement.managedReferenceValue = null;

                listProperty.serializedObject.ApplyModifiedProperties();
                GUIUtility.ExitGUI();
            }
        }

        private void DrawManagedReference_IMGUI(Rect position, SerializedProperty property, GUIContent label) {
            var fieldType = GetManagedReferenceFieldType(property);

            if (fieldType == null) {
                EditorGUI.LabelField(position, label,
                    new GUIContent($"Could not determine field type for {property.propertyPath}"));

                return;
            }

            var lineH = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var availableTypes = GetAssignableTypes(fieldType);

            var typeNames = new List<string> { "(None)" };
            typeNames.AddRange(availableTypes.Select(t => t.Name));

            var currentTypeName = property.managedReferenceValue?.GetType().Name ?? "(None)";
            var currentIndex = typeNames.IndexOf(currentTypeName);
            if (currentIndex < 0) currentIndex = 0;

            var dropdownRect = new Rect(position.x, position.y, position.width, lineH);

            EditorGUI.BeginChangeCheck();
            var newIndex = EditorGUI.Popup(dropdownRect, label.text, currentIndex, typeNames.ToArray());

            if (EditorGUI.EndChangeCheck()) {
                if (newIndex == 0)
                    property.managedReferenceValue = null;
                else {
                    var selectedType = availableTypes[newIndex - 1];
                    property.managedReferenceValue = Activator.CreateInstance(selectedType);
                }

                property.serializedObject.ApplyModifiedProperties();
            }

            if (property.managedReferenceValue == null)
                return;

            var y = position.y + lineH;

            EditorGUI.indentLevel++;

            if (IsListElement(property)) {
                // See CreateSerializeReferencePicker_UITK for the rationale — list elements don't carry the
                // [DropdownPicker] attribute, so we can defer to the element's type-specific drawer.
                y += spacing;
                var fieldH = EditorGUI.GetPropertyHeight(property, true);
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, fieldH), property, true);
            }
            else {
                EditorListHelpers.ForEachVisibleChild(property, child => {
                    y += spacing;
                    var fieldH = EditorGUI.GetPropertyHeight(child, true);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, fieldH), child, true);
                    y += fieldH;
                });
            }

            EditorGUI.indentLevel--;
        }

        private void DrawAsset_IMGUI(Rect position, SerializedProperty property, GUIContent label) {
            var fieldType = ResolveAssetFieldType();
            var assets = FindAssetsOfType(fieldType);

            var assetNames = new List<string> { "(None)" };
            assetNames.AddRange(assets.Select(a => a.name));

            var currentIndex = 0;
            var currentAsset = property.objectReferenceValue;

            if (currentAsset != null) {
                var index = assets.FindIndex(a => a == currentAsset);

                if (index >= 0)
                    currentIndex = index + 1;
            }

            EditorGUI.BeginChangeCheck();
            var newIndex = EditorGUI.Popup(position, label.text, currentIndex, assetNames.ToArray());

            if (EditorGUI.EndChangeCheck()) {
                property.objectReferenceValue = newIndex == 0 ? null : assets[newIndex - 1];
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        // ============================================================================================
        // Shared helpers
        // ============================================================================================

        /// <summary>
        /// True when the serialized property is an element inside a list / array (path contains
        /// <c>.Array.data[</c>). Used to decide whether <see cref="PropertyField"/> on the property itself is
        /// safe — list elements don't inherit the parent's [DropdownPicker] attribute, so no drawer recursion.
        /// </summary>
        private static bool IsListElement(SerializedProperty property) =>
                property.propertyPath.Contains(".Array.data[");

        private Type ResolveAssetFieldType() {
            var t = fieldInfo.FieldType;

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
                t = t.GetGenericArguments()[0];
            else if (t.IsArray)
                t = t.GetElementType();

            return t;
        }

        private Type GetManagedReferenceFieldType(SerializedProperty property) {
            var typeName = property.managedReferenceFieldTypename;

            if (string.IsNullOrEmpty(typeName))
                return null;

            var parts = typeName.Split(' ');

            if (parts.Length < 2)
                return null;

            var assemblyName = parts[0];
            var className = parts[1];

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if (assembly.GetName().Name == assemblyName || assemblyName == "Assembly-CSharp") {
                    var type = assembly.GetType(className);

                    if (type != null)
                        return type;
                }
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                var type = assembly.GetType(className);

                if (type != null)
                    return type;
            }

            return null;
        }

        private List<Type> GetAssignableTypes(Type baseType) {
            var requiredAttribute = (attribute as DropdownPickerAttribute)?.RequiredAttribute;

            return EditorListHelpers.GetAssignableTypes(baseType, requiredAttribute);
        }

        private List<UnityEngine.Object> FindAssetsOfType(Type type) {
            var typeName = type.Name;
            var guids = AssetDatabase.FindAssets($"t:{typeName}");

            return guids
                    .Select(guid => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), type))
                    .Where(a => a != null)
                    .OrderBy(a => a.name)
                    .ToList();
        }
    }
}