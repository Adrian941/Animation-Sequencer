#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
    [Serializable]
    public sealed class SetGameObjectActiveStep : AnimationStepBase
    {
        public override string DisplayName => "Set Game Object Active";

        [SerializeField]
        private GameObject target;
        public GameObject Target
        {
            get => target;
            set => target = value;
        }

        [SerializeField]
        private bool active;
        public bool Active
        {
            get => active;
            set => active = value;
        }

        private bool? originalActive;

        public override void AddTweenToSequence(Sequence animationSequence)
        {
            if (!originalActive.HasValue)
                originalActive = target.activeSelf;

            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(Delay);
            sequence.AppendInterval(0.001f);    //Interval added for a bug when this tween runs in "Backwards" direction.
            sequence.AppendCallback(() => target.SetActive(active));

            if (FlowType == FlowType.Join)
                animationSequence.Join(sequence);
            else
                animationSequence.Append(sequence);
        }

        public override void ResetToInitialState()
        {
            if (!originalActive.HasValue)
                return;

            target.SetActive(originalActive.Value);
        }

        public override string GetDisplayNameForEditor(int index)
        {
            string display = "NULL";
            if (target != null)
                display = target.name;
            
            return $"{index}. Set {display} Active: {active}";
        }
    }
}
#endif