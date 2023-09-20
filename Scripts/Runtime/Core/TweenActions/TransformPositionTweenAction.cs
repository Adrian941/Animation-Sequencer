#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class TransformPositionTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);
        public override string DisplayName => "Position";

        [SerializeField]
        private TypeInput typeInput;
        public TypeInput TypeInput
        {
            get => typeInput;
            set => typeInput = value;
        }

        [SerializeField]
        private Vector3 position;
        public Vector3 Position
        {
            get => position;
            set => position = value;
        }

        [SerializeField]
        private bool local;
        public bool Local
        {
            get => local;
            set => local = value;
        }

        [SerializeField]
        private Transform target;
        public Transform Target
        {
            get => target;
            set => target = value;
        }

        [SerializeField]
        private Vector3 offset;
        public Vector3 Offset
        {
            get => offset;
            set => offset = value;
        }

        [SerializeField]
        private AxisConstraint axisConstraint;
        public AxisConstraint AxisConstraint
        {
            get => axisConstraint;
            set => axisConstraint = value;
        }

        private Transform targetTransform;
        private Vector3 originalPosition;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            targetTransform = target.transform;

            TweenerCore<Vector3, Vector3, VectorOptions> tween;
            if (typeInput == TypeInput.Vector && local)
            {
                originalPosition = targetTransform.localPosition;
                tween = targetTransform.DOLocalMove(GetPosition(), duration);
            }
            else
            {
                originalPosition = targetTransform.position;
                tween = targetTransform.DOMove(GetPosition(), duration);
            }
            tween.SetOptions(axisConstraint);

            return tween;
        }

        private Vector3 GetPosition()
        {
            switch (typeInput)
            {
                case TypeInput.Vector:
                    return position;
                case TypeInput.Object:
                    return target.position + offset;
            }

            return Vector3.zero;
        }

        public override void ResetToInitialState()
        {
            if (targetTransform == null)
                return;

            if (typeInput == TypeInput.Vector && local)
                targetTransform.localPosition = originalPosition;
            else
                targetTransform.position = originalPosition;
        }
    }
}
#endif