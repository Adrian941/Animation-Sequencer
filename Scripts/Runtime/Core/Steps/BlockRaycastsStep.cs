#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Created by Pablo Huaxteco
    [Serializable]
    public class BlockRaycastsStep : AnimationStepBase
    {
        public override string DisplayName => "Block Raycasts";

        [SerializeField]
        private CanvasGroup target;
        public CanvasGroup Target
        {
            get => target;
            set => target = value;
        }

        [SerializeField]
        private bool toBlockRaycasts;
        public bool ToBlockRaycasts
        {
            get => toBlockRaycasts;
            set => toBlockRaycasts = value;
        }

        private bool originalBlocksRaycasts;

        public override Sequence GenerateTweenSequence()
        {
            if (target == null)
            {
                Debug.LogWarning($"The <b>\"{DisplayName}\"</b> Step does not have a <b>\"Target\"</b>. Please consider assigning a <b>\"Target\"</b> or removing the step.");
                return null;
            }

            originalBlocksRaycasts = target.blocksRaycasts;

            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(delay);

            float duration = GetExtraInterval();
            var tween = DOTween.To(() => target.blocksRaycasts ? 1f : 0f, x =>
            {
                if (x == 0f)
                    target.blocksRaycasts = false;
                else if (x == 1f)
                    target.blocksRaycasts = true;
            }
            , toBlockRaycasts ? 1f : 0f, duration);

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

            target.blocksRaycasts = originalBlocksRaycasts;
        }

        public override string GetDisplayNameForEditor(int index)
        {
            string display = "NULL";
            if (target != null)
                display = target.name;

            return $"{index}. Block \"{display}\" Raycasts: {toBlockRaycasts}";
        }

        public override float GetDuration()
        {
            return createdSequence == null ? -1 : createdSequence.Duration() - GetExtraInterval();
        }

        public override float GetExtraIntervalAdded()
        {
            return createdSequence == null ? 0 : GetExtraInterval();
        }
    }
}
#endif