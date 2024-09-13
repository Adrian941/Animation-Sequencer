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
    public sealed class CameraOrthographicSizeTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Camera);
        public override string DisplayName => "Orthographic Size";

        [SerializeField]
        [Tooltip("If TRUE the input value will be used as a percentage (e.g. 50%, 100%, 200%...)")]
        private bool percentage;
        public bool Percentage
        {
            get => percentage;
            set => percentage = value;
        }

        [SerializeField]
        private float orthographicSize = 10f;
        public float OrthographicSize
        {
            get => orthographicSize;
            set => orthographicSize = value;
        }

        private Camera targetCamera;
        private float originalOrthographicSize;

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

            if (!targetCamera.orthographic)
                Debug.Log($"{target} with {TargetComponentType} component must be of type 'Orthographic' projection to work with 'Orthographic Size' tween.");

            originalOrthographicSize = targetCamera.orthographicSize;

            float endValue = percentage ? originalOrthographicSize * (orthographicSize / 100) : orthographicSize;
            TweenerCore<float, float, FloatOptions> tween = targetCamera.DOOrthoSize(endValue, duration);

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetCamera == null)
                return;

            targetCamera.orthographicSize = originalOrthographicSize;
        }
    }
}
#endif