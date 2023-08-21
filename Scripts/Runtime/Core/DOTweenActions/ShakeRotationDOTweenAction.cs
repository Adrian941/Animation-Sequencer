#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class ShakeRotationDOTweenAction : DOTweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);
        public override string DisplayName => "Shake Rotation";

        [SerializeField]
        private Vector3 strength;
        public Vector3 Strength
        {
            get => strength;
            set => strength = value;
        }

        [SerializeField]
        private int vibrato = 10;
        public int Vibrato
        {
            get => vibrato;
            set => vibrato = value;
        }

        [SerializeField]
        private float randomness = 90;
        public float Randomness
        {
            get => randomness;
            set => randomness = value;
        }

        [SerializeField]
        private bool fadeout = true;
        public bool Fadeout
        {
            get => fadeout;
            set => fadeout = value;
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