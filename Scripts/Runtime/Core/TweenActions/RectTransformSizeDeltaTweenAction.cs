#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class RectTransformSizeDeltaTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(RectTransform);
        public override string DisplayName => "Size Delta";

        [SerializeField]
        [Tooltip("If TRUE the input value will be used as a percentage (e.g. 50%, 100%, 200%...)")]
        private bool percentage;
        public bool Percentage
        {
            get => percentage;
            set => percentage = value;
        }

        [SerializeField]
        private Vector2 sizeDelta;
        public Vector2 SizeDelta
        {
            get => sizeDelta;
            set => sizeDelta = value;
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
        private Vector2 originalSize;

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

            originalSize = targetRectTransform.sizeDelta;

            Vector3 endValue = percentage ? Vector2.Scale(originalSize, sizeDelta / 100) : sizeDelta;
            var tween = targetRectTransform.DOSizeDelta(endValue, duration);
            tween.SetOptions(axisConstraint, snapping);

            return tween;
        }

        public Vector2 GetEndValue(GameObject target)
        {
            Vector2 endValue = percentage ? Vector2.Scale((target.transform as RectTransform).sizeDelta, sizeDelta / 100) : sizeDelta;
            if (relative)
                endValue += (target.transform as RectTransform).sizeDelta;

            return endValue;
        }

        public override void ResetToInitialState()
        {
            if (targetRectTransform == null)
                return;

            targetRectTransform.sizeDelta = originalSize;
        }
    }
}
#endif