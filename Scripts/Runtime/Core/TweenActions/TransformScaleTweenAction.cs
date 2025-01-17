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
    public sealed class TransformScaleTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);
        public override string DisplayName => "Scale";

        [SerializeField]
        private Vector3 toScale;
        public Vector3 ToScale
        {
            get => toScale;
            set => toScale = value;
        }

        [SerializeField]
        [Tooltip("Enable this to interpret the input value as a percentage. Examples: 50% scales the size to half, 100% keeps it unchanged, and 200% doubles it.")]
        private bool toUseAsPercentage;
        public bool ToUseAsPercentage
        {
            get => toUseAsPercentage;
            set => toUseAsPercentage = value;
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

        private Transform targetTransform;
        private Vector3 originalScale;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            targetTransform = target.transform;
            originalScale = targetTransform.localScale;

            Vector3 endValue = toUseAsPercentage ? Vector3.Scale(originalScale, toScale / 100) : toScale;
            TweenerCore<Vector3, Vector3, VectorOptions> tween = targetTransform.DOScale(endValue, duration, axisConstraint, snapping);
            //TweenerCore<Vector3, Vector3, VectorOptions> tween = targetTransform.DOScale(endValue, duration);
            //tween.SetOptions(axisConstraint, snapping);

            return tween;
        }

        public Vector3 GetStartValue(GameObject target)
        {
            return GetValue(target, direction == AnimationDirection.To ? AnimationDirection.From : AnimationDirection.To);
        }

        public Vector3 GetEndValue(GameObject target)
        {
            return GetValue(target, direction);
        }

        private Vector3 GetValue(GameObject target, AnimationDirection direction)
        {
            return direction == AnimationDirection.To ?
                (toUseAsPercentage ? Vector3.Scale(target.transform.localScale, toScale / 100) : toScale) :
                target.transform.localScale;
        }

        protected override void ResetToInitialState_Internal()
        {
            if (targetTransform == null)
                return;

            targetTransform.localScale = originalScale;
        }
    }
}
#endif