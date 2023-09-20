#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class TransformPunchScaleTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);
        public override string DisplayName => "Punch Scale";

        [SerializeField]
        private Vector3 punch = Vector3.one;
        public Vector3 Punch
        {
            get => punch;
            set => punch = value;
        }

        [SerializeField]
        private int vibrato = 10;
        public int Vibrato
        {
            get => vibrato;
            set => vibrato = value;
        }

        [SerializeField]
        private float elasticity = 1f;
        public float Elasticity
        {
            get => elasticity;
            set => elasticity = value;
        }

        private Transform targetTransform;
        private Vector3 originalScale;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            targetTransform = target.transform;
            originalScale = targetTransform.localScale;

            Tweener tween = targetTransform.DOPunchScale(punch, duration, vibrato, elasticity);

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetTransform == null)
                return;

            targetTransform.localScale = originalScale;
        }
    }
}
#endif