#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class PlayParticleSystemAnimationStep : AnimationStepBase
    {
        public override string DisplayName => "Play Particle System";

        [SerializeField]
        private ParticleSystem particleSystem;
        public ParticleSystem ParticleSystem
        {
            get => particleSystem;
            set => particleSystem = value;
        }

        [SerializeField, Min(0)]
        private float duration = 1;
        public float Duration
        {
            get => duration;
            set => duration = Mathf.Clamp(value, 0, Mathf.Infinity);
        }

        [SerializeField]
        private bool stopEmittingWhenOver;
        public bool StopEmittingWhenOver
        {
            get => stopEmittingWhenOver;
            set => stopEmittingWhenOver = value;
        }

        public override void AddTweenToSequence(Sequence animationSequence)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(Delay);
            sequence.AppendCallback(()=>
            {
                if (!animationSequence.IsBackwards())
                    StartParticles();
                else
                    FinishParticles();

            });
            sequence.AppendInterval(duration);
            sequence.AppendCallback(()=>
            {
                if (!animationSequence.IsBackwards())
                    FinishParticles();
                else
                    StartParticles();
            });

            if (FlowType == FlowType.Join)
                animationSequence.Join(sequence);
            else
                animationSequence.Append(sequence);
        }

        public override void ResetToInitialState()
        {
        }

        private void StartParticles()
        {
            particleSystem.Play();
        }

        private void FinishParticles()
        {
            if (stopEmittingWhenOver)
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

            return $"{index}. Play {display} particle system";
        }

        public override float GetDuration()
        {
            return base.GetDuration() + duration;
        }
    }
}
#endif