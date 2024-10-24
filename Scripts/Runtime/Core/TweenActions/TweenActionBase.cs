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

        [SerializeField]
        [Tooltip("If true the end value will be calculated as (startValue + endValue) instead than being used directly.")]
        protected bool relative;
        public bool Relative
        {
            get => relative;
            set => relative = value;
        }

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
            SetTweenAnimationStep (tweenAnimationStep);
            Tweener tween = GenerateTween_Internal(target, duration);
            if (tween == null)
                return null;

            if (direction == AnimationDirection.From)
                // tween.SetRelative() does not work for From variant of "Move To Anchored Position", it must be set
                // here instead. Not sure if this is a bug in DOTween or expected behaviour...
                tween.From(isRelative: relative);

            tween.SetEase(ease);
            tween.SetRelative(relative);
            return tween;
        }

        public abstract void ResetToInitialState();
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