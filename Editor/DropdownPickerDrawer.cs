using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Stats.Editor {
    [CustomPropertyDrawer(typeof(DropdownPickerAttribute))]
    public class DropdownPickerDrawer : PropertyDrawer {
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            // Determine which mode based on property type
            if (property.propertyType == SerializedPropertyType.ManagedReference) {
                return CreateSerializeReferencePicker(property);
            }
            
            if (property.propertyType == SerializedPropertyType.ObjectReference) {
                return CreateAssetPicker(property);
            }
            
            var label = new Label($"[DropdownPicker] Unsupported property type: {property.propertyType}");
            label.style.color = Color.red;
            return label;
        }
        
        #region SerializeReference Picker (Types)
        
        private VisualElement CreateSerializeReferencePicker(SerializedProperty property) {
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
            
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 2;
            
            var label = new Label(property.displayName);
            label.style.minWidth = 150;
            
            var dropdown = new DropdownField(typeNames, currentIndex);
            dropdown.style.flexGrow = 1;
            dropdown.RegisterValueChangedCallback(evt => {
                var selectedTypeName = evt.newValue;
                
                if (selectedTypeName == "(None)") {
                    property.managedReferenceValue = null;
                } else {
                    var selectedType = availableTypes.FirstOrDefault(t => t.Name == selectedTypeName);
                    if (selectedType != null) {
                        property.managedReferenceValue = Activator.CreateInstance(selectedType);
                    }
                }
                
                property.serializedObject.ApplyModifiedProperties();
            });
            
            row.Add(label);
            row.Add(dropdown);
            container.Add(row);
            
            if (property.managedReferenceValue != null) {
                var foldout = new Foldout { text = "Properties", value = true };
                
                var iterator = property.Copy();
                var endProperty = property.GetEndProperty();
                
                if (iterator.NextVisible(true)) {
                    do {
                        if (SerializedProperty.EqualContents(iterator, endProperty))
                            break;
                        
                        var field = new PropertyField(iterator.Copy());
                        field.Bind(property.serializedObject);
                        foldout.Add(field);
                        
                    } while (iterator.NextVisible(false));
                }
                
                if (foldout.childCount > 0) {
                    container.Add(foldout);
                }
            }
            
            return container;
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
                        
                        if (!type.IsSerializable && type.GetCustomAttributes(typeof(SerializableAttribute), true).Length == 0)
                            continue;
                        
                        types.Add(type);
                    }
                } catch {
                    // Skip problematic assemblies
                }
            }
            
            return types.OrderBy(t => t.Name).ToList();
        }
        
        #endregion
        
        #region Asset Picker (ScriptableObjects)
        
        private VisualElement CreateAssetPicker(SerializedProperty property) {
            var container = new VisualElement();
            
            var fieldType = fieldInfo.FieldType;
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
            
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 2;
            
            var label = new Label(property.displayName);
            label.style.minWidth = 150;
            
            var dropdown = new DropdownField(assetNames, currentIndex);
            dropdown.style.flexGrow = 1;
            dropdown.RegisterValueChangedCallback(evt => {
                var selectedName = evt.newValue;
                
                if (selectedName == "(None)") {
                    property.objectReferenceValue = null;
                } else {
                    var selectedAsset = assets.FirstOrDefault(a => a.name == selectedName);
                    property.objectReferenceValue = selectedAsset;
                }
                
                property.serializedObject.ApplyModifiedProperties();
            });
            
            row.Add(label);
            row.Add(dropdown);
            container.Add(row);
            
            return container;
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
        
        #endregion
    }
}