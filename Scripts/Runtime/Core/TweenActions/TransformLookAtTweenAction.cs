#if DOTWEEN_ENABLED
using DG.Tweening;
using System;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Created by Pablo Huaxteco
    [Serializable]
    public sealed class TransformLookAtTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);
        public override string DisplayName => "LookAt";

        [SerializeField]
        private InputType inputType = InputType.Object;
        public InputType InputType
        {
            get => inputType;
            set => inputType = value;
        }

        [Tooltip("Position to point towards.")]
        [ShowIf("inputType", InputType.Vector)]
        [SerializeField]
        private Vector3 position;
        public Vector3 Position
        {
            get => position;
            set => position = value;
        }

        [Tooltip("Object to point towards.")]
        [ShowIf("inputType", InputType.Object)]
        [SerializeField]
        private Transform target;
        public Transform Target
        {
            get => target;
            set => target = value;
        }

        [Tooltip("The vector direction considered as 'up' (default: Vector3.up)")]
        [SerializeField]
        private VectorDirectionUtility.VectorDirection up;
        public VectorDirectionUtility.VectorDirection Up
        {
            get => up;
            set => up = value;
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

        private Transform targetTransform;
        private Quaternion originalRotation;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            targetTransform = target.transform;

            if (inputType == InputType.Object && this.target == null)
            {
                Debug.LogWarning($"The <b>\"{DisplayName}\"</b> Action does not have a <b>\"Target\"</b>. Please consider assigning a <b>\"Target\"</b>, " +
                    $"selecting another <b>\"Input Type\"</b> or removing the action.");
                return null;
            }

            originalRotation = targetTransform.rotation;
            Tweener tween = targetTransform.DOLookAt(GetPosition(), duration, axisConstraint, VectorDirectionUtility.GetDirectionVector(up));

            return tween;
        }

        private Vector3 GetPosition()
        {
            switch (inputType)
            {
                case InputType.Vector:
                    return position;
                case InputType.Object:
                    return target.position;
            }

            return Vector3.zero;
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