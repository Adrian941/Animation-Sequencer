#if DOTWEEN_ENABLED
using System;
using System.Collections.Generic;
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
        public override string[] ExcludedFields
        {
            get
            {
                List<string> result = new List<string>();

                switch (inputType)
                {
                    case InputTypeWithAnchor.Vector:
                        result.Add("target");
                        result.Add("offset");
                        result.Add("moveDirection");
                        if (relative) result.Add("local");
                        break;
                    case InputTypeWithAnchor.Object:
                        result.Add("position");
                        result.Add("local");
                        result.Add("moveDirection");
                        break;
                    case InputTypeWithAnchor.Anchor:
                        result.Add("target");
                        result.Add("position");
                        result.Add("local");
                        break;
                }

                return result.ToArray();
            }
        }

        public RectTransformAnchoredPositionTweenAction()
        {
            inputType = InputTypeWithAnchor.Anchor;
        }

        [SerializeField]
        private InputTypeWithAnchor inputType;
        public InputTypeWithAnchor InputType
        {
            get => inputType;
            set => inputType = value;
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
        private MovementDirection moveDirection;
        public MovementDirection MoveDirection
        {
            get => moveDirection;
            set => moveDirection = value;
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
        private RectTransform rootCanvasRectTransform;
        private RectTransform RootCanvasRectTransform
        {
            get
            {
                if (rootCanvasRectTransform == null)
                    rootCanvasRectTransform = targetRectTransform.GetComponentInParent<Canvas>().rootCanvas.GetComponent<RectTransform>();

                return rootCanvasRectTransform;
            }
        }
        private TweenAnimationStep tweenAnimationStep;

        protected override void SetTweenAnimationStep(TweenAnimationStep tweenAnimationStep)
        {
            this.tweenAnimationStep = tweenAnimationStep;
        }

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

            TweenerCore<Vector2, Vector2, VectorOptions> tween = targetRectTransform.DOAnchorPos(GetPosition(), duration);
            tween.SetOptions(axisConstraint, snapping);

            return tween;
        }

        private Vector2 GetPosition()
        {
            switch (inputType)
            {
                case InputTypeWithAnchor.Vector:
                    return GetPositionFromVectorInput();
                case InputTypeWithAnchor.Object:
                    return GetPositionFromObjectInput();
                case InputTypeWithAnchor.Anchor:
                    return GetPositionFromAnchorInput();
            }

            return Vector2.zero;
        }

        private Vector2 GetPositionFromVectorInput()
        {
            if (local || relative)
                return position;
            
            Vector3 targetWorldPosition = RootCanvasRectTransform.TransformPoint(position);

            return targetRectTransform.anchoredPosition + ConvertPositionFromWorldToCanvasSpace(targetWorldPosition);
        }

        private Vector2 GetPositionFromObjectInput()
        {
            return targetRectTransform.anchoredPosition + ConvertPositionFromWorldToCanvasSpace(target.position) + offset;
        }

        private Vector2 GetPositionFromAnchorInput()
        {
            Vector3[] parentCorners = new Vector3[4];
            targetRectTransform.parent.GetComponent<RectTransform>().GetWorldCorners(parentCorners);
            Vector2 anchorPosition = Vector2.zero;
            Vector2 anchorOffset = Vector2.zero;
            CalculateEndScaleAndSizeDeltaValues(out Vector3? endLocalScale, out Vector2? endSizeDelta);
            Vector2 sizeDelta = endSizeDelta.HasValue? endSizeDelta.Value : targetRectTransform.sizeDelta;
            Vector3 localScale = endLocalScale.HasValue ? endLocalScale.Value : targetRectTransform.localScale;
            Vector2 rectMiddleSize = new Vector2(sizeDelta.x / 2, sizeDelta.y / 2) * localScale;

            switch (moveDirection)
            {
                case MovementDirection.TopLeft:
                    anchorPosition = parentCorners[1];
                    anchorOffset = new Vector2(-rectMiddleSize.x, rectMiddleSize.y);
                    break;
                case MovementDirection.TopCenter:
                    anchorPosition = new Vector2((parentCorners[3].x + parentCorners[0].x) / 2, parentCorners[1].y);
                    anchorOffset = new Vector2(0, rectMiddleSize.y);
                    break;
                case MovementDirection.TopRight:
                    anchorPosition = parentCorners[2];
                    anchorOffset = new Vector2(rectMiddleSize.x, rectMiddleSize.y);
                    break;
                case MovementDirection.MiddleLeft:
                    anchorPosition = new Vector2(parentCorners[0].x, (parentCorners[1].y + parentCorners[0].y) / 2);
                    anchorOffset = new Vector2(-rectMiddleSize.x, 0);
                    break;
                case MovementDirection.MiddleCenter:
                    anchorPosition = new Vector2((parentCorners[3].x + parentCorners[0].x) / 2, (parentCorners[1].y + parentCorners[0].y) / 2);
                    break;
                case MovementDirection.MiddleRight:
                    anchorPosition = new Vector2(parentCorners[3].x, (parentCorners[1].y + parentCorners[0].y) / 2);
                    anchorOffset = new Vector2(rectMiddleSize.x, 0);
                    break;
                case MovementDirection.BottomLeft:
                    anchorPosition = parentCorners[0];
                    anchorOffset = new Vector2(-rectMiddleSize.x, -rectMiddleSize.y);
                    break;
                case MovementDirection.BottomCenter:
                    anchorPosition = new Vector2((parentCorners[3].x + parentCorners[0].x) / 2, parentCorners[3].y);
                    anchorOffset = new Vector2(0, -rectMiddleSize.y);
                    break;
                case MovementDirection.BottomRight:
                    anchorPosition = parentCorners[3];
                    anchorOffset = new Vector2(rectMiddleSize.x, -rectMiddleSize.y);
                    break;
            }
            Vector2 cornerPosition = ConvertPositionFromWorldToCanvasSpace(anchorPosition) + anchorOffset;

            return targetRectTransform.anchoredPosition + cornerPosition + offset;
        }

        /// <summary>
        /// Calculate the end scale and size delta values from all the tween animation step.
        /// </summary>
        /// <param name="endLocalScale">Returns the end scale value. Null if no "Scale" action is found.</param>
        /// <param name="endSizeDelta">Returns the end size delta value. Null if no "Size Delta" action is found.</param>
        private void CalculateEndScaleAndSizeDeltaValues(out Vector3? endLocalScale, out Vector2? endSizeDelta)
        {
            endLocalScale = null;
            endSizeDelta = null;

            if (tweenAnimationStep == null)
                return;

            TransformScaleTweenAction transformScaleTweenAction = null;
            RectTransformSizeDeltaTweenAction rectTransformSizeDeltaTweenAction = null;

            foreach (var item in tweenAnimationStep.Actions)
            {
                if (item.GetType() == typeof(TransformScaleTweenAction))
                    transformScaleTweenAction = item as TransformScaleTweenAction;
                else if (item.GetType() == typeof(RectTransformSizeDeltaTweenAction))
                    rectTransformSizeDeltaTweenAction = item as RectTransformSizeDeltaTweenAction;
            }

            if (transformScaleTweenAction != null && transformScaleTweenAction.Direction == AnimationDirection.To)
                endLocalScale = transformScaleTweenAction.GetEndValue(targetRectTransform.gameObject);

            if (rectTransformSizeDeltaTweenAction != null && rectTransformSizeDeltaTweenAction.Direction == AnimationDirection.To)
                endSizeDelta = rectTransformSizeDeltaTweenAction.GetEndValue(targetRectTransform.gameObject);
        }

        private Vector2 ConvertPositionFromWorldToCanvasSpace(Vector3 position)
        {
            //Avoid wrong calculations.
            Vector3 localEulerAngles = targetRectTransform.localEulerAngles;
            targetRectTransform.localEulerAngles = Vector3.zero;
            Vector3 localScale = targetRectTransform.localScale;
            targetRectTransform.localScale = Vector3.one;

            //Calculate new position and reset the target values.
            Vector2 targetCanvasLocalPosition = targetRectTransform.InverseTransformPoint(position);
            targetRectTransform.localEulerAngles = localEulerAngles;
            targetRectTransform.localScale = localScale;

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