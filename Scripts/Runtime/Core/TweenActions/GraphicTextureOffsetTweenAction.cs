#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.AnimationSequencer
{
    // Created by Pablo Huaxteco
    [Serializable]
    public sealed class GraphicTextureOffsetTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Graphic);
        public override string DisplayName => "Texture Offset";

        [SerializeField]
        private Vector2 offset = Vector2.one;
        public Vector2 Offset
        {
            get => offset;
            set => offset = value;
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

        private Graphic targetGraphic;
        private Vector2 originalTextureOffset;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetGraphic == null || targetGraphic.gameObject != target)
            {
                targetGraphic = target.GetComponent<Graphic>();
                if (targetGraphic == null)
                {
                    Debug.LogWarning($"The <b>\"{target.name}\"</b> GameObject does not have a <b>{TargetComponentType.Name}</b> component required  for " +
                        $"the <b>\"{DisplayName}\"</b> action. Please consider assigning a <b>{TargetComponentType.Name}</b> component or removing the action.", target);
                    return null;
                }

                //Create a clon of the current material (UI only).
                if (Application.isPlaying)
                    targetGraphic.material = UnityEngine.Object.Instantiate(targetGraphic.material);
            }

            if (Application.isPlaying)
                originalTextureOffset = targetGraphic.material.mainTextureOffset;

            TweenerCore<Vector2, Vector2, VectorOptions> tween = null;
            if (Application.isPlaying)
            {
                tween = targetGraphic.material.DOOffset(offset, duration);
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
            if (targetGraphic == null || !Application.isPlaying)
                return;

            targetGraphic.material.mainTextureOffset = originalTextureOffset;
        }
    }
}
#endif