#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Created by Pablo Huaxteco
    [Serializable]
    public sealed class TransformPositionTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);
        public override string DisplayName => "Position";

        [SerializeField]
        private InputType inputType;
        public InputType InputType
        {
            get => inputType;
            set => inputType = value;
        }

        [ShowIf("inputType", InputType.Vector)]
        [SerializeField]
        private Vector3 position;
        public Vector3 Position
        {
            get => position;
            set => position = value;
        }

        [Tooltip("If true, the tween will use local coordinates of the object, moving it relative to its parent's position and rotation. " +
            "If false, the tween will operate in world space coordinates.")]
        [ShowIf("inputType == InputType.Vector && relative == false")]
        [SerializeField]
        private bool local;
        public bool Local
        {
            get => local;
            set => local = value;
        }

        [ShowIf("inputType", InputType.Object)]
        [SerializeField]
        private Transform target;
        public Transform Target
        {
            get => target;
            set => target = value;
        }

        [ShowIf("inputType", InputType.Object)]
        [SerializeField]
        private Vector3 offset;
        public Vector3 Offset
        {
            get => offset;
            set => offset = value;
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

        [Tooltip("If true, the animated position values will snap to integer values, creating a more grid-like movement. " +
            "Useful for animations that require precise, whole number positioning.")]
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

            if (inputType == InputType.Object && this.target == null)
            {
                Debug.LogWarning($"The <b>\"{DisplayName}\"</b> Action does not have a <b>\"Target\"</b>. Please consider assigning a <b>\"Target\"</b>, " +
                    $"selecting another <b>\"Input Type\"</b> or removing the action.");
                return null;
            }

            TweenerCore<Vector3, Vector3, VectorOptions> tween;
            if (inputType == InputType.Vector && local)
            {
                originalPosition = targetTransform.localPosition;
                tween = targetTransform.DOLocalMove(GetPosition(), duration, axisConstraint, snapping);
                //tween = targetTransform.DOLocalMove(GetPosition(), duration);
            }
            else
            {
                originalPosition = targetTransform.position;
                tween = targetTransform.DOMove(GetPosition(), duration, axisConstraint, snapping);
                //tween = targetTransform.DOMove(GetPosition(), duration);
            }
            //tween.SetOptions(axisConstraint, snapping);

            return tween;
        }

        private Vector3 GetPosition()
        {
            switch (inputType)
            {
                case InputType.Vector:
                    return position;
                case InputType.Object:
                    return target.position + offset;
            }

            return Vector3.zero;
        }

        public override void ResetToInitialState()
        {
            if (targetTransform == null)
                return;

            if (inputType == InputType.Vector && local)
                targetTransform.localPosition = originalPosition;
            else
                targetTransform.position = originalPosition;
        }
    }
}
#endif