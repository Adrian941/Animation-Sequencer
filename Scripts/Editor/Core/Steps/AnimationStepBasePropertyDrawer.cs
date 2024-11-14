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

                    if (AnimationSequencerSettings.GetInstance().ShowStepAnimationInfo)
                    {
                        StepAnimationData animationData = animationStepBase.GetAnimationData();
                        float percentageDuration = animationData.percentageDuration * 100;

                        EditorGUIExtra.ProgressBar(new Rect(position.x, position.y, position.width, 54),
                            animationData.startTime / animationData.mainSequenceDuration,
                            animationData.endTime / animationData.mainSequenceDuration,
                            $"Duration: {percentageDuration.ToString(percentageDuration % 1 == 0 ? "F0" : "F2")}%",
                            "0s",
                            $"{animationData.mainSequenceDuration.ToString(animationData.mainSequenceDuration % 1 == 0 ? "F0" : "F2")}s",
                            $"{animationData.startTime.ToString(animationData.startTime % 1 == 0 ? "F0" : "F2")}s",
                            $"{animationData.endTime.ToString(animationData.endTime % 1 == 0 ? "F0" : "F2")}s");

                        position.y += 54 + EditorGUIUtility.standardVerticalSpacing;
                        DrawHorizontalLine(position);
                        position.y += EditorGUIUtility.standardVerticalSpacing * 3;
                    }

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

        private void DrawHorizontalLine(Rect position)
        {
            EditorGUI.DrawRect(new Rect(position.x, position.y, position.width, 1), Color.gray);
        }
    }
}
#endif