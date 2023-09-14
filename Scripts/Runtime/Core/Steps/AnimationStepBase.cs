#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public abstract class AnimationStepBase
    {
        [SerializeField, Min(0)]
        private float delay;
        public float Delay => delay;

        [SerializeField]
        private FlowType flowType;
        public FlowType FlowType => flowType;

        private StepAnimationData animationData;

        public abstract string DisplayName { get; }
        
        public abstract void AddTweenToSequence(Sequence animationSequence);

        public abstract void ResetToInitialState();

        public virtual string GetDisplayNameForEditor(int index)
        {
            return $"{index}. {this}";
        }

        /// <summary>
        /// Returns the duration of this step including delays and loops.
        /// </summary>
        /// <returns></returns>
        public virtual float GetDuration()
        {
            return delay;
        }

        /// <summary>
        /// Called by Editor to set animation data relative to main sequence.
        /// </summary>
        /// <param name="mainSequenceDuration">Main sequence duration (All steps duration including delays and loops).</param>
        /// <param name="startTime">The time this step starts relative to the main sequence.</param>
        public void SetAnimationData(float mainSequenceDuration, float startTime)
        {
            animationData = new StepAnimationData(mainSequenceDuration, GetDuration() / mainSequenceDuration, startTime, startTime + GetDuration());
        }

        /// <summary>
        /// Called by the CustomEditor to show a progress bar in the inspector.
        /// </summary>
        /// <returns></returns>
        public StepAnimationData GetAnimationData()
        {
            return animationData;
        }
    }

    /// <summary>
    /// Struct used by the CustomEditor to show visual animation data for each step.
    /// </summary>
    public struct StepAnimationData
    {
        /// <summary>
        /// Main sequence duration (All steps duration including delays and loops).
        /// </summary>
        public float mainSequenceDuration;
        /// <summary>
        /// Percentage duration of this step relative to the main sequence.
        /// </summary>
        public float percentageDuration;
        /// <summary>
        /// The time this step starts relative to the main sequence.
        /// </summary>
        public float startTime;
        /// <summary>
        /// The time this step ends relative to the main sequence.
        /// </summary>
        public float endTime;

        public StepAnimationData(float mainSequenceDuration, float percentageDuration, float startTime, float endTime)
        {
            this.mainSequenceDuration = mainSequenceDuration;
            this.percentageDuration = percentageDuration;
            this.startTime = startTime;
            this.endTime = endTime;
        }
    }
}
#endif