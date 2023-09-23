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
    public sealed class GraphicTextureScaleTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Graphic);
        public override string DisplayName => "Texture Scale";

        [SerializeField]
        private Vector2 scale;
        public Vector2 Scale
        {
            get => scale;
            set => scale = value;
        }

        private Graphic targetGraphic;
        private Vector2 originalTextureScale;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetGraphic == null || targetGraphic.gameObject != target)
            {
                targetGraphic = target.GetComponent<Graphic>();
                if (targetGraphic == null)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component.");
                    return null;
                }

                //Create a clon of the current material (UI only).
                if (Application.isPlaying)
                    targetGraphic.material = UnityEngine.Object.Instantiate(targetGraphic.material);
            }

            if (Application.isPlaying)
                originalTextureScale = targetGraphic.material.mainTextureScale;

            TweenerCore<Vector2, Vector2, VectorOptions> tween = null;
            if (Application.isPlaying)
            {
                tween = targetGraphic.material.DOTiling(scale, duration);
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
            if (targetGraphic == null || !Application.isPlaying)
                return;

            targetGraphic.material.mainTextureScale = originalTextureScale;
        }
    }
}
#endif