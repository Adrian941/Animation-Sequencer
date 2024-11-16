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
    public sealed class LightColorTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Light);
        public override string DisplayName => "Color";

        [SerializeField]
        private Color color = Color.white;
        public Color Color
        {
            get => color;
            set => color = value;
        }

        private Light targetLight;
        private Color originalColor;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetLight == null || targetLight.gameObject != target)
            {
                targetLight = target.GetComponent<Light>();
                if (targetLight == null)
                {
                    Debug.LogWarning($"The <b>\"{target.name}\"</b> GameObject does not have a <b>{TargetComponentType.Name}</b> component required  for " +
                        $"the <b>\"{DisplayName}\"</b> action. Please consider assigning a <b>{TargetComponentType.Name}</b> component or removing the action.", target);
                    return null;
                }
            }

            originalColor = targetLight.color;

            TweenerCore<Color, Color, ColorOptions> tween = targetLight.DOColor(color, duration);

            return tween;
        }

        protected override void ResetToInitialState_Internal()
        {
            if (targetLight == null)
                return;

            targetLight.color = originalColor;
        }
    }
}
#endif