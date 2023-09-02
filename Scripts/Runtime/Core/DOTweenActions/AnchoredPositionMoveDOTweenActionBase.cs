#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public abstract class AnchoredPositionMoveDOTweenActionBase : DOTweenActionBase
    {
        public override Type TargetComponentType => typeof(RectTransform);

        [SerializeField]
        private AxisConstraint axisConstraint;
        public AxisConstraint AxisConstraint
        {
            get => axisConstraint;
            set => axisConstraint = value;
        }

        protected RectTransform targetRectTransform;
        private Vector2 originalAnchorPosition;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetRectTransform == null || targetRectTransform.gameObject != target)
            {
                targetRectTransform = target.transform as RectTransform;

                if (targetRectTransform == null)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component");
                    return null;
                }
            }

            originalAnchorPosition = targetRectTransform.anchoredPosition;

            TweenerCore<Vector2, Vector2, VectorOptions> tween = targetRectTransform.DOAnchorPos(GetPosition(), duration);
            tween.SetOptions(axisConstraint);

            return tween;
        }

        protected abstract Vector2 GetPosition();

        public override void ResetToInitialState()
        {
            if (targetRectTransform == null)
                return;

            targetRectTransform.anchoredPosition = originalAnchorPosition;
        }
    }
}
#endif
