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

        public override Sequence GenerateTweenSequence()
        {
            if (target == null)
            {
                Debug.LogWarning($"The <b>\"{DisplayName}\"</b> Step does not have a <b>\"Target\"</b>. Please consider assigning a <b>\"Target\"</b> or removing the step.");
                return null;
            }

            if (!originalActive.HasValue)
                originalActive = target.activeSelf;

            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(Delay);
            sequence.AppendInterval(extraInterval);    //Interval added for a bug when this tween runs in "Backwards" direction.
            sequence.AppendCallback(() => target.SetActive(active));

            return sequence;
        }

        public override void ResetToInitialState()
        {
            if (target == null)
                return;

            if (!originalActive.HasValue)
                return;

            target.SetActive(originalActive.Value);
        }

        public override string GetDisplayNameForEditor(int index)
        {
            string display = "NULL";
            if (target != null)
                display = target.name;
            
            return $"{index}. Set \"{display}\" Active: {active}";
        }

        public override float GetDuration()
        {
            return sequence == null ? -1 : sequence.Duration() - extraInterval;
        }

        public override float GetExtraIntervalAdded()
        {
            return sequence == null ? 0 : extraInterval;
        }
    }
}
#endif