#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
    [Serializable]
    public abstract class AnimationStepBase
    {
        [SerializeField, Min(0)]
        private float delay;
        public float Delay => delay;
        [Tooltip("Defines how the animation flows within a sequence. 'Append' plays the animation after the previous step's animation. " +
            "'Join' plays the animation at the same time as the previous step's animation.")]
        [SerializeField]
        private FlowType flowType;
        public FlowType FlowType => flowType;
        protected Sequence sequence;
        protected readonly float extraInterval = 0.0001f; //Interval added on "Callbacks" for a bug when this tween runs in "Backwards" direction.

        public abstract string DisplayName { get; }

        /// <summary>
        /// Called by the Editor to hide fields in the inspector.
        /// </summary>
        public virtual string[] ExcludedFields => null;

        protected virtual void SetMainSequence(Sequence mainSequence) { }

        public abstract Sequence GenerateTweenSequence();

        public void AddTweenToSequence(Sequence animationSequence)
        {
            SetMainSequence(animationSequence);
            sequence = GenerateTweenSequence();
            if (sequence == null)
                return;

            if (FlowType == FlowType.Join)
                animationSequence.Join(sequence);
            else
                animationSequence.Append(sequence);
        }

        protected abstract void ResetToInitialState_Internal();

        public void ResetToInitialState()
        {
            if (sequence == null)
                return;

            ResetToInitialState_Internal();
        }

        public virtual string GetDisplayNameForEditor(int index)
        {
            return $"{index}. {this}";
        }

        public virtual float GetDuration()
        {
            return sequence == null ? -1 : sequence.Duration();
        }

        public virtual float GetExtraIntervalAdded()
        {
            return 0;
        }
    }
}
#endif