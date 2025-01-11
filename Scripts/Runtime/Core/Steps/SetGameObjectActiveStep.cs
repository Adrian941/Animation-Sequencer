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
        private bool toActive;
        public bool ToActive
        {
            get => toActive;
            set => toActive = value;
        }

        private bool originalActiveSelf;

        public override Sequence GenerateTweenSequence()
        {
            if (target == null)
            {
                Debug.LogWarning($"The <b>\"{DisplayName}\"</b> Step does not have a <b>\"Target\"</b>. Please consider assigning a <b>\"Target\"</b> or removing the step.");
                return null;
            }

            originalActiveSelf = target.activeSelf;

            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(Delay);

            float duration = GetExtraInterval();
            var tween = DOTween.To(() => target.activeSelf ? 1f : 0f, x =>
            {
                if (x == 0f)
                    target.SetActive(false);
                else if (x == 1f)
                    target.SetActive(true);
            }
            , toActive ? 1f : 0f, duration);

            sequence.Append(tween);

            return sequence;
        }

        private float GetExtraInterval()
        {
            return extraInterval;
        }

        protected override void ResetToInitialState_Internal()
        {
            if (target == null)
                return;

            target.SetActive(originalActiveSelf);
        }

        public override string GetDisplayNameForEditor(int index)
        {
            string display = "NULL";
            if (target != null)
                display = target.name;
            
            return $"{index}. Set \"{display}\" Active: {toActive}";
        }

        public override float GetDuration()
        {
            return sequence == null ? -1 : sequence.Duration() - GetExtraInterval();
        }

        public override float GetExtraIntervalAdded()
        {
            return sequence == null ? 0 : GetExtraInterval();
        }
    }
}
#endif