#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
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

        [SerializeField]
        [Tooltip("Particles will stop emitting when the duration is over.")]
        private bool setLifetime;
        public bool SetLifetime
        {
            get => setLifetime;
            set => setLifetime = value;
        }

        [ShowIf("setLifetime")]
        [SerializeField, Min(0)]
        private float duration = 1;
        public float Duration
        {
            get => duration;
            set => duration = Mathf.Clamp(value, 0, Mathf.Infinity);
        }

        public override void AddTweenToSequence(Sequence animationSequence)
        {
            if (particleSystem == null)
            {
                Debug.LogWarning($"The <b>\"{DisplayName}\"</b> Step does not have a <b>\"Target\"</b>. Please consider assigning a <b>\"Target\"</b> or removing the step.");
                return;
            }

            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(Delay);
            sequence.AppendCallback(()=>
            {
                if (!animationSequence.IsBackwards())
                    StartParticles();
                else
                    FinishParticles();
            });

            if (setLifetime) sequence.AppendInterval(duration);

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

        public override void ResetToInitialState() { }

        private void StartParticles()
        {
            particleSystem.Play();
        }

        private void FinishParticles()
        {
            if (setLifetime)
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

            return $"{index}. Play \"{display}\" particle system";
        }

        public override float GetDuration()
        {
            return base.GetDuration() + (setLifetime ? duration : 0);
        }
    }
}
#endif