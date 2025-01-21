#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Created by Pablo Huaxteco
    [Serializable]
    public sealed class RectTransformAnchoredPositionTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(RectTransform);
        public override string DisplayName => "Anchored Position";

        public RectTransformAnchoredPositionTweenAction()
        {
            toInputType = InputTypeWithAnchor.Anchor;
            toAnchorPosition = AnchorPosition.MiddleCenter;
        }

        [SerializeField]
        private InputTypeWithAnchor toInputType;
        public InputTypeWithAnchor ToInputType
        {
            get => toInputType;
            set => toInputType = value;
        }

        [ShowIf("toInputType", InputTypeWithAnchor.Vector)]
        [SerializeField]
        private Vector2 toPosition;
        public Vector2 ToPosition
        {
            get => toPosition;
            set => toPosition = value;
        }

        [Tooltip("If true, the tween will use local coordinates of the object, moving it relative to its parent's position and rotation. " +
            "If false, the tween will operate in world space coordinates.")]
        [ShowIf("toInputType == InputTypeWithAnchor.Vector")]
        [SerializeField]
        private bool toLocal = true;
        public bool ToLocal
        {
            get => toLocal;
            set => toLocal = value;
        }

        [ShowIf("toInputType", InputTypeWithAnchor.Object)]
        [SerializeField]
        private RectTransform toTarget;
        public RectTransform ToTarget
        {
            get => toTarget;
            set => toTarget = value;
        }

        [SerializeField]
        private AnchorPosition toAnchorPosition;
        public AnchorPosition ToAnchorPosition
        {
            get => toAnchorPosition;
            set => toAnchorPosition = value;
        }

        [ShowIf("toInputType != InputTypeWithAnchor.Vector")]
        [SerializeField]
        private Vector2 toOffset;
        public Vector2 ToOffset
        {
            get => toOffset;
            set => toOffset = value;
        }

        [Tooltip("Specifies the axis or combination of axes along which the animation will apply. " +
            "Use this to constrain movement to a single axis (X, Y, or Z) or a combination of them.")]
        [SerializeField]
        private AxisConstraint axisConstraint;
        public AxisConstraint AxisConstraint
        {
            get => axisConstraint;
            set => axisConstraint = value;
        }

        [Tooltip("If true, the animated position values will snap to integer values, creating a more grid-like movement. " +
            "Useful for animations that require precise, whole number positioning.")]
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
                    Debug.LogWarning($"The <b>\"{target.name}\"</b> GameObject does not have a <b>{TargetComponentType.Name}</b> component required  for " +
                        $"the <b>\"{DisplayName}\"</b> action. Please consider assigning a <b>{TargetComponentType.Name}</b> component or removing the action.", target);
                    return null;
                }
            }

            if (toInputType == InputTypeWithAnchor.Object && this.toTarget == null)
            {
                Debug.LogWarning($"The <b>\"{DisplayName}\"</b> Action does not have a <b>\"Target\"</b>. Please consider assigning a <b>\"Target\"</b>, " +
                    $"selecting another <b>\"Input Type\"</b> or removing the action.");
                return null;
            }

            originalAnchorPosition = targetRectTransform.anchoredPosition;

            TweenerCore<Vector2, Vector2, VectorOptions> tween = targetRectTransform.DOAnchorPos(GetPosition(), duration);
            tween.SetOptions(axisConstraint, snapping);

            return tween;
        }

        private Vector2 GetPosition()
        {
            switch (toInputType)
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
            if (toLocal)
                return toPosition;
            
            Vector3 targetWorldPosition = RootCanvasRectTransform.TransformPoint(toPosition);

            return targetRectTransform.anchoredPosition + ConvertPositionFromWorldToCanvasSpace(targetWorldPosition);
        }

        private Vector2 GetPositionFromObjectInput()
        {
            return targetRectTransform.anchoredPosition + ConvertPositionFromWorldToCanvasSpace(toTarget.position) + toOffset;
        }

        private Vector2 GetPositionFromAnchorInput()
        {
            Vector3[] parentCorners = new Vector3[4];
            targetRectTransform.parent.GetComponent<RectTransform>().GetWorldCorners(parentCorners);
            Vector2 anchorPosition = Vector2.zero;
            Vector2 anchorOffset = Vector2.zero;
            CalculateEndScaleAndSizeDeltaValues(out Vector3? endLocalScale, out Vector2? endSizeDelta);
            Vector2 sizeDelta = endSizeDelta.HasValue ? endSizeDelta.Value : targetRectTransform.rect.size;
            Vector3 localScale = endLocalScale.HasValue ? endLocalScale.Value : targetRectTransform.localScale;
            Quaternion rotation = targetRectTransform.localRotation;
            if (rotation != Quaternion.identity)
                sizeDelta = GetRotatedSize(sizeDelta, rotation);
            Vector2 rectMiddleSize = sizeDelta / 2 * localScale;

            switch (toAnchorPosition)
            {
                case AnchorPosition.TopLeft:
                    anchorPosition = parentCorners[1];
                    anchorOffset = new Vector2(-rectMiddleSize.x, rectMiddleSize.y);
                    break;
                case AnchorPosition.TopCenter:
                    anchorPosition = (parentCorners[1] + parentCorners[2]) / 2;
                    anchorOffset = new Vector2(0, rectMiddleSize.y);
                    break;
                case AnchorPosition.TopRight:
                    anchorPosition = parentCorners[2];
                    anchorOffset = new Vector2(rectMiddleSize.x, rectMiddleSize.y);
                    break;
                case AnchorPosition.MiddleLeft:
                    anchorPosition = (parentCorners[0] + parentCorners[1]) / 2;
                    anchorOffset = new Vector2(-rectMiddleSize.x, 0);
                    break;
                case AnchorPosition.MiddleCenter:
                    anchorPosition = (parentCorners[0] + parentCorners[2]) / 2;
                    break;
                case AnchorPosition.MiddleRight:
                    anchorPosition = (parentCorners[2] + parentCorners[3]) / 2;
                    anchorOffset = new Vector2(rectMiddleSize.x, 0);
                    break;
                case AnchorPosition.BottomLeft:
                    anchorPosition = parentCorners[0];
                    anchorOffset = new Vector2(-rectMiddleSize.x, -rectMiddleSize.y);
                    break;
                case AnchorPosition.BottomCenter:
                    anchorPosition = (parentCorners[0] + parentCorners[3]) / 2;
                    anchorOffset = new Vector2(0, -rectMiddleSize.y);
                    break;
                case AnchorPosition.BottomRight:
                    anchorPosition = parentCorners[3];
                    anchorOffset = new Vector2(rectMiddleSize.x, -rectMiddleSize.y);
                    break;
            }
            Vector2 cornerPosition = ConvertPositionFromWorldToCanvasSpace(anchorPosition) + anchorOffset;

            return targetRectTransform.anchoredPosition + cornerPosition + toOffset;
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

            if (transformScaleTweenAction != null)
            {
                if (direction == AnimationDirection.To)
                    endLocalScale = transformScaleTweenAction.GetEndValue(targetRectTransform.gameObject);
                else
                    endLocalScale = transformScaleTweenAction.GetStartValue(targetRectTransform.gameObject);
            }

            if (rectTransformSizeDeltaTweenAction != null)
            {
                if (direction == AnimationDirection.To)
                    endSizeDelta = rectTransformSizeDeltaTweenAction.GetEndValue(targetRectTransform);
                else
                    endSizeDelta = rectTransformSizeDeltaTweenAction.GetStartValue(targetRectTransform);
            }
        }

        private Vector2 GetRotatedSize(Vector2 size, Quaternion rotation)
        {
            Vector3[] rotatedCorners = CalculateRotatedCorners(size, rotation);
            return GetBoundingBoxSize(rotatedCorners);
        }

        private Vector3[] CalculateRotatedCorners(Vector2 size, Quaternion rotation)
        {
            Vector3[] corners = new Vector3[4];

            // Original corners in local coordinates (before rotation)
            Vector3 topLeft = new Vector2(-size.x / 2, size.y / 2);
            Vector3 topRight = new Vector2(size.x / 2, size.y / 2);
            Vector3 bottomLeft = new Vector2(-size.x / 2, -size.y / 2);
            Vector3 bottomRight = new Vector2(size.x / 2, -size.y / 2);

            // Apply rotation
            corners[0] = rotation * topLeft;
            corners[1] = rotation * topRight;
            corners[2] = rotation * bottomRight;
            corners[3] = rotation * bottomLeft;

            return corners;
        }

        private Vector2 GetBoundingBoxSize(Vector3[] corners)
        {
            float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

            return new Vector2(maxX - minX, maxY - minY);
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

        protected override void ResetToInitialState_Internal()
        {
            if (targetRectTransform == null)
                return;

            targetRectTransform.anchoredPosition = originalAnchorPosition;
        }
    }
}
#endif