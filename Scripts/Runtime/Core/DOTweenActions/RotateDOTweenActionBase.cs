#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public abstract class RotateDOTweenActionBase : DOTweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);
        public override string DisplayName => "Punch Scale";

        [SerializeField]
        private bool local;
        public bool Local
        {
            get => local;
            set => local = value;
        }

        [SerializeField]
        private RotateMode rotationMode = RotateMode.Fast;
        public RotateMode RotationMode
        {
            get => rotationMode;
            set => rotationMode = value;
        }

        private Transform targetTransform;
        private Quaternion originalRotation;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            targetTransform = target.transform;

            TweenerCore<Quaternion, Vector3, QuaternionOptions> tween;
            if (local)
            {
                originalRotation = targetTransform.localRotation;
                tween = targetTransform.DOLocalRotate(GetRotation(), duration, rotationMode);
            }
            else
            {
                originalRotation = targetTransform.rotation;
                tween = targetTransform.DORotate(GetRotation(), duration, rotationMode);
            }

            return tween;
        }


        protected abstract Vector3 GetRotation();

        public override void ResetToInitialState()
        {
            if (targetTransform == null)
                return;

            if (!local)
                targetTransform.rotation = originalRotation;
            else
                targetTransform.localRotation = originalRotation;
        }
    }
}
#endif