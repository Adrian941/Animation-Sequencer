#if DOTWEEN_ENABLED
using System;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class AnchoredPositionMoveToPositionDOTweenActionBase : AnchoredPositionMoveDOTweenActionBase
    {
        public override string DisplayName => "Move To Anchored Position";

        [SerializeField]
        private Vector2 position;
        public Vector2 Position
        {
            get => position;
            set => position = value;
        }

        [SerializeField]
        private bool local = true;
        public bool Local
        {
            get => local;
            set => local = value;
        }

        private Canvas parentCanvas;

        protected override Vector2 GetPosition()
        {
            if (local)
                return position;

            if (parentCanvas == null)
                parentCanvas = targetRectTransform.GetComponentInParent<Canvas>().rootCanvas;

            Vector3 targetWorldPosition = ((RectTransform)parentCanvas.transform).TransformPoint(position);
            Vector2 targetCanvasLocalPosition = targetRectTransform.InverseTransformPoint(targetWorldPosition);

            return targetRectTransform.anchoredPosition + targetCanvasLocalPosition;
        }
    }
}
#endif