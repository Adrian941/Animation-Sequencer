#if DOTWEEN_ENABLED
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Created by Pablo Huaxteco
    [CustomPropertyDrawer(typeof(TransformPathTweenAction), true)]
    public class TransformPathTweenActionPropertyDrawer : TweenActionBasePropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawBaseGUI(position, property, label, "direction");
        }

        protected override bool ShouldShowProperty(SerializedProperty currentProperty, SerializedProperty property)
        {
            if (currentProperty.name == "positions")
                return IsInputTypeSelected(property, InputType.Vector);

            if (currentProperty.name == "targets")
                return IsInputTypeSelected(property, InputType.Object);

            return base.ShouldShowProperty(currentProperty, property);
        }

        private bool IsInputTypeSelected(SerializedProperty property, InputType inputType)
        {
            SerializedProperty inputTypeSerializedProperty = property.FindPropertyRelative("inputType");
            InputType selectedInputType = (InputType)inputTypeSerializedProperty.enumValueIndex;

            return selectedInputType == inputType;
        }
    }
}
#endif