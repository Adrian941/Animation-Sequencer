#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
    [Serializable]
    public sealed class InvokeCallbackAnimationStep : AnimationStepBase
    {
        public override string DisplayName => "Invoke Callback";

        [SerializeField]
        private UnityEvent callback = new UnityEvent();
        public UnityEvent Callback
        {
            get => callback;
            set => callback = value;
        }

        public override Sequence GenerateTweenSequence()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(Delay);
            sequence.AppendInterval(extraInterval);    //Interval added for a bug when this tween runs in "Backwards" direction.
            sequence.AppendCallback(callback.Invoke);
            
            return sequence;
        }

        protected override void ResetToInitialState_Internal() { }

        public override string GetDisplayNameForEditor(int index)
        {
            string[] persistentTargetNamesArray = new string[callback.GetPersistentEventCount()];
            for (int i = 0; i < callback.GetPersistentEventCount(); i++)
            {
                if (callback.GetPersistentTarget(i) == null)
                    continue;
                
                if (string.IsNullOrWhiteSpace(callback.GetPersistentMethodName(i)))
                    continue;
                
                persistentTargetNamesArray[i] = $"{callback.GetPersistentTarget(i).name}.{callback.GetPersistentMethodName(i)}()";
            }

            var persistentTargetNames = $"{string.Join(", ", persistentTargetNamesArray)}";
            
            return $"{index}. {DisplayName}: {persistentTargetNames}";
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