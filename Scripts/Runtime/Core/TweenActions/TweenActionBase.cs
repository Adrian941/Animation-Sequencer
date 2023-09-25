#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public abstract class TweenActionBase
    {
        public enum AnimationDirection
        {
            To,
            From
        }

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
        [Tooltip("If TRUE the endValue will be calculated as startValue + endValue instead than being used directly.")]
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

        protected abstract Tweener GenerateTween_Internal(GameObject target, float duration);

        public Tween GenerateTween(GameObject target, float duration)
        {
            Tweener tween = GenerateTween_Internal(target, duration);
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

    public enum TypeInput
    {
        Vector,
        Object
    }
}
#endif