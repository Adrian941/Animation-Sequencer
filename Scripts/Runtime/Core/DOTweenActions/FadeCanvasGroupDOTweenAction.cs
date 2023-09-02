#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class FadeCanvasGroupDOTweenAction : DOTweenActionBase
    {
        public override Type TargetComponentType => typeof(CanvasGroup);
        public override string DisplayName => "Fade Canvas Group";

        [SerializeField, Range(0, 1)]
        private float alpha;
        public float Alpha
        {
            get => alpha;
            set => alpha = Mathf.Clamp01(value);
        }

        private CanvasGroup targetCanvasGroup;
        private float originalAlpha;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetCanvasGroup == null || targetCanvasGroup.gameObject != target)
            {
                targetCanvasGroup = target.GetComponent<CanvasGroup>();
                if (targetCanvasGroup == null)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component");
                    return null;
                }
            }

            originalAlpha = targetCanvasGroup.alpha;

            TweenerCore<float, float, FloatOptions> tween = targetCanvasGroup.DOFade(alpha, duration);

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetCanvasGroup == null)
                return;

            targetCanvasGroup.alpha = originalAlpha;
        }
    }
}
#endif