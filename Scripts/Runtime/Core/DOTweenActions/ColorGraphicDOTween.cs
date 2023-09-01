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
    public sealed class ColorGraphicDOTween : DOTweenActionBase
    {
        public override Type TargetComponentType => typeof(Graphic);
        public override string DisplayName => "Color Graphic";

        [SerializeField]
        private Color color;
        public Color Color
        {
            get => color;
            set => color = value;
        }

        private Graphic targetGraphic;
        private Color originalColor;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetGraphic == null)
            {
                targetGraphic = target.GetComponent<Graphic>();
                if (targetGraphic == null)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component");
                    return null;
                }
            }

            originalColor = targetGraphic.color;

            TweenerCore<Color, Color, ColorOptions> tween = targetGraphic.DOColor(color, duration);
            
            return tween;
        }
        
        public override void ResetToInitialState()
        {
            if (targetGraphic == null)
                return;

            targetGraphic.color = originalColor;
        }
    }
}
#endif