#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class PlaySequenceAnimationStep : AnimationStepBase
    {
        public override string DisplayName => "Play Sequence";

        [SerializeField]
        private AnimationSequencerController sequencer;
        public AnimationSequencerController Sequencer
        {
            get => sequencer;
            set => sequencer = value;
        }

        public override void AddTweenToSequence(Sequence animationSequence)
        {
            if (sequencer == null)
            {
                Debug.LogWarning($"One <b>\"{DisplayName}\"</b> Step is null and will not be considered in the animation. Please assign or remove it.");
                return;
            }

            Sequence sequence = sequencer.GenerateSequence();
            sequence.SetDelay(Delay);

            if (FlowType == FlowType.Join)
                animationSequence.Join(sequence);
            else
                animationSequence.Append(sequence);
        }

        public override void ResetToInitialState()
        {
            if (sequencer == null)
                return;

            sequencer.ResetToInitialState();
        }

        public override string GetDisplayNameForEditor(int index)
        {
            string display = "NULL";
            if (sequencer != null)
                display = sequencer.name;

            return $"{index}. Play \"{display}\" Sequence";
        }

        public void SetTarget(AnimationSequencerController newTarget)
        {
            sequencer = newTarget;
        }
    }
}
#endif