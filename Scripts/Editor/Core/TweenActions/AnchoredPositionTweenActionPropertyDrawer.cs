#if DOTWEEN_ENABLED
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [CustomPropertyDrawer(typeof(TransformPathTweenAction), true)]
    [CustomPropertyDrawer(typeof(TransformPositionTweenAction), true)]
    [CustomPropertyDrawer(typeof(RectTransformAnchoredPositionTweenAction), true)]
    public sealed class AnchoredPositionTweenActionPropertyDrawer : TweenActionBasePropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty typeInputSerializedProperty = property.FindPropertyRelative("typeInput");
            TypeInput typeInput = (TypeInput)typeInputSerializedProperty.enumValueIndex;

            if (typeInput == TypeInput.Vector)
                DrawBaseGUI(position, property, label, "target", "offset", "targets");
            else
                DrawBaseGUI(position, property, label, "position", "local", "positions");
        }
    }
}
#endif