#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class AudioSourcePitchTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(AudioSource);
        public override string DisplayName => "Pitch";

        [SerializeField, Range(-3f, 3f)]
        private float pitch = 1.5f;
        public float Pitch
        {
            get => pitch;
            set => pitch = Mathf.Clamp(value, -3f, 3f);
        }

        private AudioSource targetAudioSource;
        private float originalPitch;

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

            originalPitch = targetAudioSource.pitch;

            TweenerCore<float, float, FloatOptions> tween = targetAudioSource.DOPitch(pitch, duration);

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetAudioSource == null)
                return;

            targetAudioSource.pitch = originalPitch;
        }
    }
}
#endif