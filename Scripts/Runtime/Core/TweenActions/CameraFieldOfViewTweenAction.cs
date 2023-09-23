#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class CameraFieldOfViewTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Camera);
        public override string DisplayName => "Field Of View";

        [SerializeField, Range(0, 179)]
        private float fieldOfView = 120f;
        public float FieldOfView
        {
            get => fieldOfView;
            set => fieldOfView = Mathf.Clamp(value, 0, 179);
        }

        private Camera targetCamera;
        private float originalFieldOfView;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetCamera == null || targetCamera.gameObject != target)
            {
                targetCamera = target.GetComponent<Camera>();
                if (targetCamera == null)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component.");
                    return null;
                }
            }

            if (targetCamera.orthographic)
                Debug.Log($"{target} with {TargetComponentType} component must be of type 'Perspective' projection to work with 'Field Of View' tween.");

            originalFieldOfView = targetCamera.fieldOfView;

            TweenerCore<float, float, FloatOptions> tween = targetCamera.DOFieldOfView(fieldOfView, duration);

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetCamera == null)
                return;

            targetCamera.fieldOfView = originalFieldOfView;
        }
    }
}
#endif