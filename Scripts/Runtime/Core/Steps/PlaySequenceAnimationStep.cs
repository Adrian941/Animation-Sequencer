﻿#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
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

        public override Sequence GenerateTweenSequence()
        {
            if (sequencer == null)
            {
                Debug.LogWarning($"The <b>\"{DisplayName}\"</b> Step does not have a <b>\"Target\"</b>. Please consider assigning a <b>\"Target\"</b> or removing the step.");
                return null;
            }

            if(!sequencer.isActiveAndEnabled)
                return null;

            //Sequence sequence = sequencer.GenerateSequence();
            sequencer.PlayForward(true);
            sequencer.PlayingSequence.Pause();
            Sequence sequence = sequencer.PlayingSequence;
            sequence.SetDelay(Delay);

            return sequence;
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

        public override float GetDuration()
        {
            return sequence == null ? -1 : sequence.Duration() - sequencer.ExtraIntervalAdded;
        }

        public override float GetExtraIntervalAdded()
        {
            return sequence == null ? 0 : sequencer.ExtraIntervalAdded;
        }
    }
}
#endif