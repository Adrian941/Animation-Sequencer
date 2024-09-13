#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Created by Pablo Huaxteco
    [Serializable]
    public sealed class TransformShakePositionTweenAction : TransformShakeBaseTweenAction
    {
        public override string DisplayName => "Shake Position";

        [SerializeField]
        private bool snapping;
        public bool Snapping
        {
            get => snapping;
            set => snapping = value;
        }

        private Transform targetTransform;
        private Vector3 originalPosition;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            targetTransform = target.transform;
            originalPosition = targetTransform.position;

            Tweener tween = targetTransform.DOShakePosition(duration, strength, vibrato, randomness, snapping, fadeout);

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetTransform == null)
                return;

            targetTransform.position = originalPosition;
        }
    }
}
#endif