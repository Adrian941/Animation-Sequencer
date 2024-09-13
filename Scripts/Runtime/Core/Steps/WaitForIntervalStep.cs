#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
    [Serializable]
    public sealed class WaitForIntervalStep : AnimationStepBase
    {
        public override string DisplayName => "Wait for Interval";

        [SerializeField, Min(0)]
        private float interval;
        public float Interval
        {
            get => interval;
            set => interval = Mathf.Clamp(value, 0, Mathf.Infinity);
        }

        public override void AddTweenToSequence(Sequence animationSequence)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(Delay);
            sequence.AppendInterval(interval);

            if (FlowType == FlowType.Join)
                animationSequence.Join(sequence);
            else
                animationSequence.Append(sequence);
        }

        public override void ResetToInitialState()
        {
        }

        public override string GetDisplayNameForEditor(int index)
        {
            return $"{index}. Wait {interval} seconds";
        }

        public override float GetDuration()
        {
            return base.GetDuration() + interval;
        }
    }
}
#endif