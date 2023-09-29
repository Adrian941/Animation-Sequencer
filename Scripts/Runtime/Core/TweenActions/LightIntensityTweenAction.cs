#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class LightIntensityTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Light);
        public override string DisplayName => "Intensity";

        [SerializeField, Min(0)]
        private float intensity;
        public float Intensity
        {
            get => intensity;
            set => intensity = Mathf.Clamp(value, 0, Mathf.Infinity);
        }

        private Light targetLight;
        private float originalIntensity;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetLight == null || targetLight.gameObject != target)
            {
                targetLight = target.GetComponent<Light>();
                if (targetLight == null)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component.");
                    return null;
                }
            }

            originalIntensity = targetLight.intensity;

            TweenerCore<float, float, FloatOptions> tween = targetLight.DOIntensity(intensity, duration);

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetLight == null)
                return;

            targetLight.intensity = originalIntensity;
        }
    }
}
#endif