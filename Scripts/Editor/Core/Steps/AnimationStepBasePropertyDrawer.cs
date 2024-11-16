#if DOTWEEN_ENABLED
using System;
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
    [CustomPropertyDrawer(typeof(AnimationStepBase), true)]
    public class AnimationStepBasePropertyDrawer : PropertyDrawer
    {
        protected void DrawBaseGUI(Rect position, SerializedProperty property, GUIContent label, params string[] excludedPropertiesNames)
        {
            float originY = position.y;

            position.height = EditorGUIUtility.singleLineHeight;

            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, EditorStyles.foldout);

            if (property.isExpanded)
            {
                EditorGUI.BeginChangeCheck();

                //EditorGUI.indentLevel++;
                position = EditorGUI.IndentedRect(position);
                //EditorGUI.indentLevel--;

                position.height = EditorGUIUtility.singleLineHeight;
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                if (property.TryGetTargetObjectOfProperty(out AnimationStepBase animationStepBase))
                {
                    if (animationStepBase.FlowType == FlowType.Join)
                        EditorGUI.indentLevel--;

                    string[] excludedFields = animationStepBase.ExcludedFields;
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

                bool isExtraStandardVerticalSpacingAdded = false;
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

                    EditorGUI.PropertyField(position, serializedProperty);
                    position.y += EditorGUI.GetPropertyHeight(serializedProperty) + EditorGUIUtility.standardVerticalSpacing;
                    isExtraStandardVerticalSpacingAdded = true;
                }

                if (isExtraStandardVerticalSpacingAdded)
                    position.y -= EditorGUIUtility.standardVerticalSpacing;

                if (EditorGUI.EndChangeCheck())
                    property.serializedObject.ApplyModifiedProperties();
            }

            property.SetPropertyDrawerHeight(position.y - originY + (property.isExpanded ? 0 : EditorGUIUtility.singleLineHeight));
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