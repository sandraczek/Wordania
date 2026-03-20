using UnityEditor;
using UnityEngine;
using Wordania.Core.Attributes;

namespace Wordania.Core.Editor
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.LabelField(position, label.text, "Use [Layer] with int only!");
                return;
            }

            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }
    }
}