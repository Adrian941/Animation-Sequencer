#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class RendererTextureScaleTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Renderer);
        public override string DisplayName => "Texture Scale";

        [SerializeField]
        private Vector2 scale;
        public Vector2 Scale
        {
            get => scale;
            set => scale = value;
        }

        private Renderer targetRenderer;
        private Vector2 originalTextureScale;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetRenderer == null || targetRenderer.gameObject != target)
            {
                targetRenderer = target.GetComponent<Renderer>();
                if (targetRenderer == null)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component.");
                    return null;
                }
            }

            if (Application.isPlaying)
                originalTextureScale = targetRenderer.material.mainTextureScale;

            TweenerCore<Vector2, Vector2, VectorOptions> tween = null;
            if (Application.isPlaying)
            {
                tween = targetRenderer.material.DOTiling(scale, duration);
            }
            else
            {
                Vector2 myVector = Vector2.zero;
                tween = DOTween.To(() => myVector, x => myVector = x, Vector2.one, duration);
            }

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetRenderer == null || !Application.isPlaying)
                return;

            targetRenderer.material.mainTextureScale = originalTextureScale;
        }
    }
}
#endif