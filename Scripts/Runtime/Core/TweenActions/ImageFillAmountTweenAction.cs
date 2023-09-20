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
    public sealed class ImageFillAmountTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Image);
        public override string DisplayName => "Fill Amount";

        [SerializeField, Range(0, 1)]
        private float fillAmount;
        public float FillAmount
        {
            get => fillAmount;
            set => fillAmount = Mathf.Clamp01(value);
        }

        private Image targetImage;
        private float originalFillAmount;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetImage == null || targetImage.gameObject != target)
            {
                targetImage = target.GetComponent<Image>();
                if (targetImage == null)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component");
                    return null;
                }
            }

            if (targetImage.type != Image.Type.Filled)
                Debug.Log($"{target} with {TargetComponentType} component must be of type 'Filled' to work with 'Fill Amount' tween");

            originalFillAmount = targetImage.fillAmount;

            TweenerCore<float, float, FloatOptions> tween = targetImage.DOFillAmount(fillAmount, duration);

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetImage == null)
                return;

            targetImage.fillAmount = originalFillAmount;
        }
    }
}
#endif