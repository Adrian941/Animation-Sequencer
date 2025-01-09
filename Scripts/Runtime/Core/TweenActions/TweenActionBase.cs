#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
    [Serializable]
    public abstract class TweenActionBase
    {
        public enum AnimationDirection
        {
            To,
            From
        }

        [Tooltip("Specifies the direction of the animation: 'To' animates towards the end value, 'From' animates from the end value back to the start value.")]
        [SerializeField]
        protected AnimationDirection direction;
        public AnimationDirection Direction
        {
            get => direction;
            set => direction = value;
        }

        [SerializeField]
        protected CustomEase ease = CustomEase.InOutCirc;
        public CustomEase Ease
        {
            get => ease;
            set => ease = value;
        }

        protected bool isTweenGenerated;

        public virtual Type TargetComponentType { get; }
        public abstract string DisplayName { get; }
        /// <summary>
        /// Called by the Editor to hide fields in the inspector like "direction" or "relative" (Some tweens have no "direction" effect).
        /// </summary>
        public virtual string[] ExcludedFields => null;

        protected virtual void SetTweenAnimationStep(TweenAnimationStep tweenAnimationStep) { }

        protected abstract Tweener GenerateTween_Internal(GameObject target, float duration);

        public Tween GenerateTween(GameObject target, float duration, TweenAnimationStep tweenAnimationStep = null)
        {
            SetTweenAnimationStep(tweenAnimationStep);
            Tweener tween = GenerateTween_Internal(target, duration);
            isTweenGenerated = tween != null;
            if (!isTweenGenerated)
                return null;

            if (direction == AnimationDirection.From)
                tween.From();

            tween.SetEase(ease);
            return tween;
        }

        protected abstract void ResetToInitialState_Internal();

        public void ResetToInitialState()
        {
            if (!isTweenGenerated)
                return;

            ResetToInitialState_Internal();
        }
    }

    public enum InputType
    {
        Vector,
        Object
    }

    public enum InputTypeWithAnchor
    {
        Vector,
        Object,
        Anchor
    }

    public enum MovementDirection
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
}
#endif