#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class TransformShakeScaleTweenAction : TransformShakeBaseTweenAction
    {
        public override string DisplayName => "Shake Scale";

        private Transform targetTransform;
        private Vector3 originalScale;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            targetTransform = target.transform;
            originalScale = targetTransform.localScale;

            Tweener tween = targetTransform.DOShakeScale(duration, strength, vibrato, randomness, fadeout);

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