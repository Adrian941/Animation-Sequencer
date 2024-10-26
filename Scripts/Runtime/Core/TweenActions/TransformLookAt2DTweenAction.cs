#if DOTWEEN_ENABLED
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Created by Pablo Huaxteco
    [Serializable]
    public class TransformLookAt2DTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);
        public override string[] ExcludedFields => new string[] { "relative" };
        public override string DisplayName => "LookAt 2D";

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
        private Vector2 position;
        public Vector2 Position
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
            TweenerCore<Quaternion, Vector3, QuaternionOptions> tween = targetTransform.DORotate(CalculateRotation(), duration);

            return tween;
        }

        private Vector3 CalculateRotation()
        {
            // Calculates the direction towards the target.
            Vector2 direction = GetPosition() - (Vector2)targetTransform.position;

            // Gets the angle in radians and converts it to degrees.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Applies the rotation on the Z axis (since it's a 2D space).
            return new Vector3(0, 0, angle);
        }

        private Vector2 GetPosition()
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