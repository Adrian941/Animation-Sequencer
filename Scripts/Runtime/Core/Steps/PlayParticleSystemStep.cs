#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
    [Serializable]
    public sealed class PlayParticleSystemStep : AnimationStepBase
    {
        public override string DisplayName => "Play Particles";

        [SerializeField]
        private ParticleSystem particleSystem;
        public ParticleSystem ParticleSystem
        {
            get => particleSystem;
            set => particleSystem = value;
        }

        [SerializeField]
        private bool toPlay = true;
        public bool ToPlay
        {
            get => toPlay;
            set => toPlay = value;
        }

        private Sequence mainSequence;
        private bool originalIsEmitting;

        protected override void SetMainSequence(Sequence mainSequence)
        {
            this.mainSequence = mainSequence;
        }

        public override Sequence GenerateTweenSequence()
        {
            if (particleSystem == null)
            {
                Debug.LogWarning($"The <b>\"{DisplayName}\"</b> Step does not have a <b>\"Target\"</b>. Please consider assigning a <b>\"Target\"</b> or removing the step.");
                return null;
            }

            originalIsEmitting = particleSystem.isEmitting;

            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(delay);

            float duration = GetExtraInterval();
            var tween = DOTween.To(() => particleSystem.isEmitting ? 1f : 0f, x =>
            {
                if (x == 0f)
                    particleSystem.Stop();
                else if (x == 1f)
                    particleSystem.Play();
            }
            , toPlay ? 1f : 0f, duration);

            sequence.Append(tween);

            return sequence;
        }

        private float GetExtraInterval()
        {
            return extraInterval;
        }

        protected override void ResetToInitialState_Internal() 
        {
            if (particleSystem == null)
                return;

            if (originalIsEmitting)
                particleSystem.Play();
            else
                particleSystem.Stop();
        }

        public void SetTarget(ParticleSystem newTarget)
        {
            particleSystem = newTarget;
        }

        public override string GetDisplayNameForEditor(int index)
        {
            string display = "NULL";
            if (particleSystem != null)
                display = particleSystem.name;

            return $"{index}. Play \"{display}\" particles";
        }

        public override float GetDuration()
        {
            return createdSequence == null ? -1 : createdSequence.Duration() - GetExtraInterval();
        }

        public override float GetExtraIntervalAdded()
        {
            return createdSequence == null ? 0 : GetExtraInterval();
        }
    }
}
#endif