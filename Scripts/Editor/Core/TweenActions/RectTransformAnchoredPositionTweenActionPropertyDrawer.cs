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
                DataInputTypeWithAnchor inputType = (DataInputTypeWithAnchor)inputTypeSerializedProperty.enumValueIndex;

                return inputType == DataInputTypeWithAnchor.Anchor;
            }

            return base.ShouldShowProperty(currentProperty, property);
        }
    }
}
#endif