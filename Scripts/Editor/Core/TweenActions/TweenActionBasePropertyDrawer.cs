#if DOTWEEN_ENABLED
using System;
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [CustomPropertyDrawer(typeof(TweenActionBase), true)]
    public class TweenActionBasePropertyDrawer : PropertyDrawer
    {
        protected void DrawBaseGUI(Rect position, SerializedProperty property, GUIContent label, params string[] excludedPropertiesNames)
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

            if (property.TryGetTargetObjectOfProperty(out TweenActionBase tweenActionBase))
            {
                string[] excludedFields = tweenActionBase.ExcludedFields;
                if (excludedFields != null && excludedFields.Length > 0)
                {
                    if (excludedPropertiesNames.Length > 0)
                    {
                        string[] tempPropertiesNames = new string[excludedPropertiesNames.Length + excludedFields.Length];
                        excludedPropertiesNames.CopyTo(tempPropertiesNames, 0);
                        excludedFields.CopyTo(tempPropertiesNames, excludedPropertiesNames.Length);
                        excludedPropertiesNames = tempPropertiesNames;
                    }
                    else
                    {
                        excludedPropertiesNames = excludedFields;
                    }
                }
            }

            foreach (SerializedProperty serializedProperty in property.GetChildren())
            {
                bool shouldDraw = true;
                for (int i = 0; i < excludedPropertiesNames.Length; i++)
                {
                    string excludedPropertyName = excludedPropertiesNames[i];
                    if (serializedProperty.name.Equals(excludedPropertyName, StringComparison.Ordinal))
                    {
                        shouldDraw = false;
                        break;
                    }
                }

                if (!shouldDraw)
                    continue;

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

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawBaseGUI(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.GetPropertyDrawerHeight();
        }
    }
}
#endif