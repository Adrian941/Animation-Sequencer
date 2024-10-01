#if DOTWEEN_ENABLED
using System;
using System.Collections.Generic;
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
        public override string[] ExcludedFields
        {
            get
            {
                List<string> result = new List<string>();

                if (inputType == InputType.Vector)
                {
                    result.Add("target");
                    result.Add("offset");
                    if (relative) result.Add("local");
                }
                else
                {
                    result.Add("position");
                    result.Add("local");
                }

                return result.ToArray();
            }
        }

        [SerializeField]
        private InputType inputType;
        public InputType InputType
        {
            get => inputType;
            set => inputType = value;
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
                tween = targetTransform.DOLocalMove(GetPosition(), duration);
            }
            else
            {
                originalPosition = targetTransform.position;
                tween = targetTransform.DOMove(GetPosition(), duration);
            }
            tween.SetOptions(axisConstraint, snapping);

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