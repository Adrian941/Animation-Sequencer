#if DOTWEEN_ENABLED
using System;
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [CustomPropertyDrawer(typeof(DOTweenActionBase), true)]
    public sealed class DOTweenActionBasePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float originY = position.y;

            Type type = property.GetTypeFromManagedFullTypeName();
            
            GUIContent displayName = AnimationSequenceEditorGUIUtility.GetTypeDisplayName(type);

            position.x += 10;
            position.width -= 20;
            
            EditorGUI.BeginProperty(position, GUIContent.none, property);

            float startingYPosition = position.y;

            //-26 = "X" button size.
            EditorGUI.LabelField(new Rect(position.x, position.y, position.width - 26, position.height), displayName, EditorStyles.boldLabel);

            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.BeginChangeCheck();

            foreach (SerializedProperty serializedProperty in property.GetChildren())
            {
                Rect propertyRect = position;
                EditorGUI.PropertyField(propertyRect, serializedProperty, true);

                position.y += EditorGUI.GetPropertyHeight(serializedProperty, true) + EditorGUIUtility.standardVerticalSpacing;
            }
            
            position.x -= 10;
            position.width += 20;

            Rect boxPosition = position;
            boxPosition.y = startingYPosition - 10;
            boxPosition.height = (position.y - startingYPosition) + 20;
            GUI.Box(boxPosition, GUIContent.none, EditorStyles.helpBox);
            
            EditorGUI.EndProperty();
            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();
            
            property.SetPropertyDrawerHeight(position.y - originY);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.GetPropertyDrawerHeight();
        }
    }
}
#endif