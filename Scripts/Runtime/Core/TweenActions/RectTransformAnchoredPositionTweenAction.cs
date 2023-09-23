#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class RectTransformAnchoredPositionTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(RectTransform);
        public override string DisplayName => "Anchored Position";

        [SerializeField]
        private TypeInput typeInput;
        public TypeInput TypeInput
        {
            get => typeInput;
            set => typeInput = value;
        }

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

        [SerializeField]
        private RectTransform target;
        public RectTransform Target
        {
            get => target;
            set => target = value;
        }

        [SerializeField]
        private Vector2 offset;
        public Vector2 Offset
        {
            get => offset;
            set => offset = value;
        }

        [SerializeField]
        private AxisConstraint axisConstraint;
        public AxisConstraint AxisConstraint
        {
            get => axisConstraint;
            set => axisConstraint = value;
        }

        [SerializeField]
        private bool snapping;
        public bool Snapping
        {
            get => snapping;
            set => snapping = value;
        }

        private RectTransform targetRectTransform;
        private Vector2 originalAnchorPosition;
        private Canvas parentCanvas;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetRectTransform == null || targetRectTransform.gameObject != target)
            {
                targetRectTransform = target.transform as RectTransform;

                if (targetRectTransform == null)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component.");
                    return null;
                }
            }

            originalAnchorPosition = targetRectTransform.anchoredPosition;

            TweenerCore<Vector2, Vector2, VectorOptions> tween = targetRectTransform.DOAnchorPos(GetPosition(), duration, snapping);
            tween.SetOptions(axisConstraint);

            return tween;
        }

        private Vector2 GetPosition()
        {
            switch (typeInput)
            {
                case TypeInput.Vector:
                    return GetPositionFromVectorInput();
                case TypeInput.Object:
                    return GetPositionFromObjectInput();
            }

            return Vector2.zero;
        }

        private Vector2 GetPositionFromVectorInput()
        {
            if (local)
                return position;

            if (parentCanvas == null)
                parentCanvas = targetRectTransform.GetComponentInParent<Canvas>().rootCanvas;
            Vector3 targetWorldPosition = ((RectTransform)parentCanvas.transform).TransformPoint(position);

            return targetRectTransform.anchoredPosition + ConvertPositionFromWorldToCanvasSpace(targetWorldPosition);
        }

        private Vector2 GetPositionFromObjectInput()
        {
            return targetRectTransform.anchoredPosition + ConvertPositionFromWorldToCanvasSpace(target.position) + offset;
        }

        private Vector2 ConvertPositionFromWorldToCanvasSpace(Vector3 position)
        {
            Vector2 targetCanvasLocalPosition = targetRectTransform.InverseTransformPoint(position);
            targetCanvasLocalPosition.x *= targetRectTransform.localScale.x;
            targetCanvasLocalPosition.y *= targetRectTransform.localScale.y;

            return targetCanvasLocalPosition;
        }

        public override void ResetToInitialState()
        {
            if (targetRectTransform == null)
                return;

            targetRectTransform.anchoredPosition = originalAnchorPosition;
        }
    }
}
#endif