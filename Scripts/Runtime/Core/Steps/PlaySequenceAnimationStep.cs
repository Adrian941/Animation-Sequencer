#if DOTWEEN_ENABLED
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
        private AnimationSequencer sequencer;
        public AnimationSequencer Sequencer
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

            if (!sequencer.IsActiveAndEnabled)
                return null;

            //Sequence sequence = sequencer.GenerateSequence();
            sequencer.PlayForward(true);
            sequencer.PlayingSequence.Pause();
            Sequence sequence = sequencer.PlayingSequence;
            sequence.SetDelay(delay);

            return sequence;
        }

        protected override void ResetToInitialState_Internal()
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

        public void SetTarget(AnimationSequencer newTarget)
        {
            sequencer = newTarget;
        }

        public override float GetDuration()
        {
            return createdSequence == null ? -1 : createdSequence.Duration() - sequencer.ExtraIntervalAdded;
        }

        public override float GetExtraIntervalAdded()
        {
            return createdSequence == null ? 0 : sequencer.ExtraIntervalAdded;
        }
    }
}
#endif