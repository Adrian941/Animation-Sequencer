#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class TransformScaleTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);
        public override string DisplayName => "Scale";

        [SerializeField]
        private Vector3 scale;
        public Vector3 Scale
        {
            get => scale;
            set => scale = value;
        }

        [SerializeField]
        private AxisConstraint axisConstraint;
        public AxisConstraint AxisConstraint
        {
            get => axisConstraint;
            set => axisConstraint = value;
        }

        private Transform targetTransform;
        private Vector3? originalScale;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            targetTransform = target.transform;
            originalScale = targetTransform.localScale;

            TweenerCore<Vector3, Vector3, VectorOptions> tween = targetTransform.DOScale(scale, duration).SetEase(ease);
            tween.SetOptions(axisConstraint);

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (!originalScale.HasValue)
                return;

            targetTransform.localScale = originalScale.Value;
        }
    }
}
#endif