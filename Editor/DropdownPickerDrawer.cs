// Editor/Drawers/SerializeReferencePickerDrawer.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Spellbound.Stats.Editor {
    [CustomPropertyDrawer(typeof(DropdownPickerAttribute))]
    public class DropdownPickerDrawer : PropertyDrawer {
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var container = new VisualElement();
            
            // Get the type of the field (e.g., SbBehaviour, IModifier)
            var fieldType = GetFieldType(property);
            
            if (fieldType == null) {
                container.Add(new Label($"Could not determine field type for {property.propertyPath}"));
                return container;
            }
            
            // Find all concrete types that implement/extend this type
            var availableTypes = GetAssignableTypes(fieldType);
            
            // Build dropdown choices
            var typeNames = new List<string> { "(None)" };
            typeNames.AddRange(availableTypes.Select(t => t.Name));
            
            // Current value's type
            var currentTypeName = property.managedReferenceValue?.GetType().Name ?? "(None)";
            var currentIndex = typeNames.IndexOf(currentTypeName);
            if (currentIndex < 0) currentIndex = 0;
            
            // Create horizontal layout
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginBottom = 2
                }
            };

            // Type dropdown
            var dropdown = new DropdownField(typeNames, currentIndex)
            {
                style =
                {
                    minWidth = 150
                }
            };
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
            
            row.Add(new Label(property.displayName));
            row.Add(dropdown);
            container.Add(row);
            
            // If there's a value, show its properties
            if (property.managedReferenceValue == null) 
                return container;
            
            var foldout = new Foldout
            {
                text = "Properties", 
                value = true
            };
                
            // Iterate child properties
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
                
            container.Add(foldout);

            return container;
        }
        
        private Type GetFieldType(SerializedProperty property) {
            // Get the type from the managed reference field type string
            // Format is "Assembly TypeName"
            var typeName = property.managedReferenceFieldTypename;
            
            if (string.IsNullOrEmpty(typeName))
                return null;
            
            // Split into assembly and type
            var parts = typeName.Split(' ');
            if (parts.Length < 2)
                return null;
            
            var assemblyName = parts[0];
            var className = parts[1];
            
            // Find the type
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if (assembly.GetName().Name == assemblyName || assemblyName == "Assembly-CSharp") {
                    var type = assembly.GetType(className);
                    if (type != null)
                        return type;
                }
            }
            
            // Fallback: search all assemblies
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
                        
                        // Must have parameterless constructor
                        if (type.GetConstructor(Type.EmptyTypes) == null)
                            continue;
                        
                        // Must be serializable
                        if (!type.IsSerializable && type.GetCustomAttributes(typeof(SerializableAttribute), true).Length == 0)
                            continue;
                        
                        types.Add(type);
                    }
                } catch {
                    // Some assemblies can't be reflected, skip them
                }
            }
            
            return types.OrderBy(t => t.Name).ToList();
        }
    }
}