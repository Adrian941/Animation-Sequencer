#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class TransformShakeRotationTweenAction : TransformShakeBaseTweenAction
    {
        public override string DisplayName => "Shake Rotation";

        public TransformShakeRotationTweenAction()
        {
            strength = new Vector3 (90, 90, 90);
        }

        private Transform targetTransform;
        private Quaternion originalRotation;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            targetTransform = target.transform;
            originalRotation = targetTransform.rotation;

            Tweener tween = targetTransform.DOShakeRotation(duration, strength, vibrato, randomness, fadeout);

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetTransform == null)
                return;

            targetTransform.rotation = originalRotation;
        }
    }
}
#endif