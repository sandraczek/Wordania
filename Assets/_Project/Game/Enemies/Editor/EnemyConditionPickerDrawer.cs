#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Wordania.Enemies.AI.Conditions;

namespace Wordania.Enemies.Editor
{
    /// <summary>
    /// Type dropdown for [SerializeReference] conditions: lists every non-abstract <see cref="EnemyCondition"/>,
    /// so a new condition class shows up in profiles automatically.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnemyConditionPickerAttribute))]
    public sealed class EnemyConditionPickerDrawer : PropertyDrawer
    {
        private static Type[] _types;
        private static GUIContent[] _labels;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (property.propertyType != SerializedPropertyType.ManagedReference) return height;
            if (property.managedReferenceValue == null || !property.isExpanded) return height;

            foreach (SerializedProperty child in Children(property))
                height += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.LabelField(position, label.text, "Use [SerializeReference] with [EnemyConditionPicker]");
                return;
            }

            EnsureTypes();
            EditorGUI.BeginProperty(position, label, property);

            Rect line = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect labelRect = new(line.x, line.y, EditorGUIUtility.labelWidth, line.height);
            Rect popupRect = new(line.x + EditorGUIUtility.labelWidth, line.y, line.width - EditorGUIUtility.labelWidth, line.height);

            bool hasValue = property.managedReferenceValue != null;
            if (hasValue)
                property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, true);
            else
                EditorGUI.LabelField(labelRect, label);

            Type current = property.managedReferenceValue?.GetType();
            int index = current == null ? 0 : Array.IndexOf(_types, current) + 1;

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            int selected = EditorGUI.Popup(popupRect, index, _labels);
            EditorGUI.indentLevel = indent;

            if (selected != index)
            {
                property.managedReferenceValue = selected == 0 ? null : Activator.CreateInstance(_types[selected - 1]);
                property.isExpanded = true;
                property.serializedObject.ApplyModifiedProperties();
            }

            if (hasValue && property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float y = line.yMax + EditorGUIUtility.standardVerticalSpacing;
                foreach (SerializedProperty child in Children(property))
                {
                    float h = EditorGUI.GetPropertyHeight(child, true);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), child, true);
                    y += h + EditorGUIUtility.standardVerticalSpacing;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private static IEnumerable<SerializedProperty> Children(SerializedProperty property)
        {
            SerializedProperty iterator = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            if (!iterator.NextVisible(true)) yield break;

            while (!SerializedProperty.EqualContents(iterator, end))
            {
                yield return iterator.Copy();
                if (!iterator.NextVisible(false)) yield break;
            }
        }

        private static void EnsureTypes()
        {
            if (_types != null) return;

            _types = TypeCache.GetTypesDerivedFrom<EnemyCondition>()
                .Where(t => !t.IsAbstract && !t.IsGenericType && t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(t => t.Name)
                .ToArray();

            _labels = new GUIContent[_types.Length + 1];
            _labels[0] = new GUIContent("(Always)");
            for (int i = 0; i < _types.Length; i++)
            {
                string name = _types[i].Name;
                if (name.EndsWith("Condition")) name = name[..^"Condition".Length];
                _labels[i + 1] = new GUIContent(ObjectNames.NicifyVariableName(name));
            }
        }
    }
}
#endif
