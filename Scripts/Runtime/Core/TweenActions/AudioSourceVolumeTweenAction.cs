#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class AudioSourceVolumeTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(AudioSource);
        public override string DisplayName => "Volume";

        [SerializeField, Range(0, 1)]
        private float volume;
        public float Volume
        {
            get => volume;
            set => volume = Mathf.Clamp01(value);
        }

        private AudioSource targetAudioSource;
        private float originalVolume;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetAudioSource == null || targetAudioSource.gameObject != target)
            {
                targetAudioSource = target.GetComponent<AudioSource>();
                if (targetAudioSource == null)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component.");
                    return null;
                }
            }

            originalVolume = targetAudioSource.volume;

            TweenerCore<float, float, FloatOptions> tween = targetAudioSource.DOFade(volume, duration);

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetAudioSource == null)
                return;

            targetAudioSource.volume = originalVolume;
        }
    }
}
#endif