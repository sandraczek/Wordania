using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wordania.Attributes.Editor
{
    /// <summary>
    /// Custom property drawer for fields marked with [SubclassSelector].
    /// Renders a dropdown for polymorphic selection and correctly iterates 
    /// through child properties to render the actual data fields.
    /// </summary>
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Base height for the dropdown row
            float height = EditorGUIUtility.singleLineHeight;

            // If the foldout is expanded and we have an instantiated object, add the height of all children
            if (property.isExpanded && property.managedReferenceValue != null)
            {
                SerializedProperty iterator = property.Copy();
                SerializedProperty endProperty = iterator.GetEndProperty();

                // Move to the first child
                iterator.NextVisible(true);

                while (!SerializedProperty.EqualContents(iterator, endProperty))
                {
                    height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                    // Move to the next sibling (false means don't dig into children of children yet, GetPropertyHeight handles that)
                    iterator.NextVisible(false);
                }
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 1. Draw the header row (Foldout + Dropdown)
            Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            Rect foldoutRect = new Rect(headerRect.x, headerRect.y, EditorGUIUtility.labelWidth, headerRect.height);
            Rect buttonRect = new Rect(headerRect.x + EditorGUIUtility.labelWidth, headerRect.y, headerRect.width - EditorGUIUtility.labelWidth, headerRect.height);

            // Draw the foldout (the little arrow)
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            string currentTypeName = property.managedReferenceValue != null
                ? property.managedReferenceValue.GetType().Name
                : "Null (Click to assign)";

            if (GUI.Button(buttonRect, new GUIContent(currentTypeName), EditorStyles.popup))
            {
                ShowTypeMenu(property);
            }

            // 2. Draw the children fields if expanded
            if (property.isExpanded && property.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;

                Rect childRect = new Rect(position.x, headerRect.yMax + EditorGUIUtility.standardVerticalSpacing, position.width, 0);

                SerializedProperty iterator = property.Copy();
                SerializedProperty endProperty = iterator.GetEndProperty();

                iterator.NextVisible(true);

                while (!SerializedProperty.EqualContents(iterator, endProperty))
                {
                    childRect.height = EditorGUI.GetPropertyHeight(iterator, true);

                    // Draw the child property (TargetStat, Value, etc.)
                    EditorGUI.PropertyField(childRect, iterator, true);

                    childRect.y += childRect.height + EditorGUIUtility.standardVerticalSpacing;
                    iterator.NextVisible(false);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private void ShowTypeMenu(SerializedProperty property)
        {
            GenericMenu menu = new GenericMenu();
            Type baseType = GetBaseType(property);

            if (baseType == null)
            {
                Debug.LogError("[SubclassSelector] Could not determine base type for reference.");
                return;
            }

            menu.AddItem(new GUIContent("Null"), property.managedReferenceValue == null, () =>
            {
                AssignNewInstance(property, null);
            });

            menu.AddSeparator("");

            var derivedTypes = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.Name);

            foreach (Type type in derivedTypes)
            {
                bool isSelected = property.managedReferenceValue != null && property.managedReferenceValue.GetType() == type;

                menu.AddItem(new GUIContent(type.Name), isSelected, () =>
                {
                    AssignNewInstance(property, type);
                });
            }

            menu.ShowAsContext();
        }

        private void AssignNewInstance(SerializedProperty property, Type targetType)
        {
            property.serializedObject.Update();

            object instance = targetType != null ? Activator.CreateInstance(targetType) : null;
            property.managedReferenceValue = instance;

            // Auto-expand the foldout when a new item is created so the user immediately sees the fields
            property.isExpanded = instance != null;

            property.serializedObject.ApplyModifiedProperties();
        }

        private Type GetBaseType(SerializedProperty property)
        {
            string typeName = property.managedReferenceFieldTypename;
            if (string.IsNullOrEmpty(typeName)) return null;

            string[] parts = typeName.Split(' ');
            if (parts.Length != 2) return null;

            string assemblyName = parts[0];
            string fullTypeName = parts[1];

            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            return assembly?.GetType(fullTypeName);
        }
    }
}