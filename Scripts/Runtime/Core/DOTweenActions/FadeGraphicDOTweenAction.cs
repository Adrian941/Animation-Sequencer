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
    public sealed class FadeGraphicDOTweenAction : DOTweenActionBase
    {
        public override Type TargetComponentType => typeof(Graphic);
        public override string DisplayName => "Fade Graphic";

        [SerializeField, Range(0, 1)]
        private float alpha;
        public float Alpha
        {
            get => alpha;
            set => alpha = Mathf.Clamp01(value);
        }

        private Graphic targetGraphic;
        private float originalAlpha;

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

            originalAlpha = targetGraphic.color.a;

            TweenerCore<Color, Color, ColorOptions> tween = targetGraphic.DOFade(alpha, duration);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Work around a Unity bug where updating the colour does not cause any visual change outside of PlayMode.
                // https://forum.unity.com/threads/editor-scripting-force-color-update.798663/
                tween.OnUpdate(() =>
                {
                    targetGraphic.transform.localScale = new Vector3(1.001f, 1.001f, 1.001f);
                    targetGraphic.transform.localScale = new Vector3(1, 1, 1);
                });
            }
#endif

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetGraphic == null)
                return;

            Color color = targetGraphic.color;
            color.a = originalAlpha;
            targetGraphic.color = color;
        }
    }
}
#endif