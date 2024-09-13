#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Created by Pablo Huaxteco
    [Serializable]
    public sealed class SpriteRendererColorTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(SpriteRenderer);
        public override string DisplayName => "Color";

        [SerializeField]
        private Color color = Color.white;
        public Color Color
        {
            get => color;
            set => color = value;
        }

        private SpriteRenderer targetSpriteRenderer;
        private Color originalColor;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetSpriteRenderer == null || targetSpriteRenderer.gameObject != target)
            {
                targetSpriteRenderer = target.GetComponent<SpriteRenderer>();
                if (targetSpriteRenderer == null)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component.");
                    return null;
                }
            }

            originalColor = targetSpriteRenderer.color;

            TweenerCore<Color, Color, ColorOptions> tween = targetSpriteRenderer.DOColor(color, duration);

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetSpriteRenderer == null)
                return;

            targetSpriteRenderer.color = originalColor;
        }
    }
}
#endif