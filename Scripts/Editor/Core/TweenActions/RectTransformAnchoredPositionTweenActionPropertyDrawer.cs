#if DOTWEEN_ENABLED
using UnityEditor;

namespace BrunoMikoski.AnimationSequencer
{
    // Created by Pablo Huaxteco
    [CustomPropertyDrawer(typeof(RectTransformAnchoredPositionTweenAction), true)]
    public class RectTransformAnchoredPositionTweenActionPropertyDrawer : TweenActionBasePropertyDrawer
    {
        protected override bool ShouldShowProperty(SerializedProperty currentProperty, SerializedProperty property)
        {
            if (currentProperty.name == "toAnchorPosition")
            {
                SerializedProperty inputTypeSerializedProperty = property.FindPropertyRelative("toInputType");
                InputTypeWithAnchor inputType = (InputTypeWithAnchor)inputTypeSerializedProperty.enumValueIndex;

                return inputType == InputTypeWithAnchor.Anchor;
            }

            return base.ShouldShowProperty(currentProperty, property);
        }
    }
}
#endif