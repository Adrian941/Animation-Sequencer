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
    public sealed class RendererTextureOffsetTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Renderer);
        public override string DisplayName => "Texture Offset";

        [SerializeField]
        private Vector2 toOffset = Vector2.one;
        public Vector2 ToOffset
        {
            get => toOffset;
            set => toOffset = value;
        }

        [Tooltip("Specifies the axis or combination of axes along which the animation will apply. " +
            "Use this to constrain movement to a single axis (X, Y, or Z) or a combination of them.")]
        [SerializeField]
        private AxisConstraint axisConstraint;
        public AxisConstraint AxisConstraint
        {
            get => axisConstraint;
            set => axisConstraint = value;
        }

        private Renderer targetRenderer;
        private Vector2 originalTextureOffset;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetRenderer == null || targetRenderer.gameObject != target)
            {
                targetRenderer = target.GetComponent<Renderer>();
                if (targetRenderer == null)
                {
                    Debug.LogWarning($"The <b>\"{target.name}\"</b> GameObject does not have a <b>{TargetComponentType.Name}</b> component required  for " +
                        $"the <b>\"{DisplayName}\"</b> action. Please consider assigning a <b>{TargetComponentType.Name}</b> component or removing the action.", target);
                    return null;
                }
            }

            if (Application.isPlaying)
                originalTextureOffset = targetRenderer.material.mainTextureOffset;

            TweenerCore<Vector2, Vector2, VectorOptions> tween = null;
            if (Application.isPlaying)
            {
                tween = targetRenderer.material.DOOffset(toOffset, duration);
            }
            else
            {
                Vector2 myVector = Vector2.zero;
                tween = DOTween.To(() => myVector, x => myVector = x, Vector2.one, duration);
            }
            tween.SetOptions(axisConstraint);

            return tween;
        }

        protected override void ResetToInitialState_Internal()
        {
            if (targetRenderer == null || !Application.isPlaying)
                return;

            targetRenderer.material.mainTextureOffset = originalTextureOffset;
        }
    }
}
#endif