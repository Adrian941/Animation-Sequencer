#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class SliderValueTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Slider);
        public override string DisplayName => "Value";

        [SerializeField]
        private float value;
        public float Value
        {
            get => value;
            set => this.value = value;
        }

        [SerializeField]
        private bool snapping;
        public bool Snapping
        {
            get => snapping;
            set => snapping = value;
        }

        private Slider targetSlider;
        private float originalValue;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetSlider == null || targetSlider.gameObject != target)
            {
                targetSlider = target.GetComponent<Slider>();
                if (targetSlider == null)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component.");
                    return null;
                }
            }

            originalValue = targetSlider.value;

            TweenerCore<float, float, FloatOptions> tween = targetSlider.DOValue(value, duration, snapping);

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetSlider == null)
                return;

            targetSlider.value = originalValue;
        }
    }
}
#endif