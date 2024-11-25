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

        public override Sequence GenerateTweenSequence()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(Delay);
            sequence.AppendInterval(interval);

            return sequence;
        }

        public override void ResetToInitialState() { }

        public override string GetDisplayNameForEditor(int index)
        {
            return $"{index}. Wait {Delay + interval} seconds";
        }
    }
}
#endif