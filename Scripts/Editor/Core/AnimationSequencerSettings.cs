using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
    public sealed class AnimationSequencerSettings : ScriptableObjectForPreferences<AnimationSequencerSettings>
    {
        // Serialized fields
        [Header("While Editing")]
        [SerializeField]
        private bool oneStepExpanded = true;
        [Header("When Previewing")]
        [SerializeField]
        private bool hideSteps;
        [SerializeField]
        private bool collapseSteps = true;
        [SerializeField]
        private bool visualizeStepsProgress = true;

        // Public properties
        public bool OneStepExpandedWhileEditing => oneStepExpanded;
        public bool HideStepsWhenPreviewing => hideSteps;
        public bool CollapseStepsWhenPreviewing { get { return hideSteps ? false : collapseSteps; } }
        public bool VisualizeStepsProgressWhenPreviewing { get { return hideSteps ? false : visualizeStepsProgress; } }

        [SettingsProvider]
        private static SettingsProvider SettingsProvider()
        {
            return CreateSettingsProvider("Preferences/Animation Sequencer", OnGUI);
        }

        private static void OnGUI(SerializedObject serializedObject)
        {
            // Initial margin.
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            // Modify label width.
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 190;

            // Draw properties.
            SerializedProperty oneStepExpandeProperty = serializedObject.FindProperty("oneStepExpanded");
            EditorGUILayout.PropertyField(oneStepExpandeProperty);
            SerializedProperty hideStepsProperty = serializedObject.FindProperty("hideSteps");
            EditorGUILayout.PropertyField(hideStepsProperty);
            if (!hideStepsProperty.boolValue)
            {
                int baseIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = baseIndentLevel + 1;
                SerializedProperty collapseStepsProperty = serializedObject.FindProperty("collapseSteps");
                SerializedProperty visualizeStepsProgressProperty = serializedObject.FindProperty("visualizeStepsProgress");
                EditorGUILayout.PropertyField(collapseStepsProperty);
                EditorGUILayout.PropertyField(visualizeStepsProgressProperty);
                EditorGUI.indentLevel = baseIndentLevel;
            }

            // Reset label width.
            EditorGUIUtility.labelWidth = originalLabelWidth;

            // End vertical block.
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}
