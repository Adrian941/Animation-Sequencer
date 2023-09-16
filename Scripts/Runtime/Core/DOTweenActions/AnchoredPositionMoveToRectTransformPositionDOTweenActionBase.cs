#if DOTWEEN_ENABLED
using System;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class AnchoredPositionMoveToRectTransformPositionDOTweenActionBase : AnchoredPositionMoveDOTweenActionBase
    {
        public override string DisplayName => "Move To RectTransform Anchored Position";

        [SerializeField]
        private RectTransform target;
        public RectTransform Target
        {
            get => target;
            set => target = value;
        }

        protected override Vector2 GetPosition()
        {
            Vector2 targetCanvasLocalPosition = targetRectTransform.InverseTransformPoint(target.position);
            targetCanvasLocalPosition.x *= targetRectTransform.localScale.x;
            targetCanvasLocalPosition.y *= targetRectTransform.localScale.y;

            return targetRectTransform.anchoredPosition + targetCanvasLocalPosition;
        }
    }
}
#endif
