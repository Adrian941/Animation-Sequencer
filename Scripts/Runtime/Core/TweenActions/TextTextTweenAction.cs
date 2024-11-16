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
    public sealed class TextTextTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Text);
        public override string DisplayName => "Text (Normal)";

        [SerializeField]
        private string text;
        public string Text
        {
            get => text;
            set => text = value;
        }

        [SerializeField]
        private bool richText;
        public bool RichText
        {
            get => richText;
            set => richText = value;
        }

        [SerializeField]
        private ScrambleMode scrambleMode = ScrambleMode.None;
        public ScrambleMode ScrambleMode
        {
            get => scrambleMode;
            set => scrambleMode = value;
        }

        private Text targetText;
        private string originalText;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetText == null || targetText.gameObject != target)
            {
                targetText = target.GetComponent<Text>();
                if (targetText == null)
                {
                    Debug.LogWarning($"The <b>\"{target.name}\"</b> GameObject does not have a <b>{TargetComponentType.Name}</b> component required  for " +
                        $"the <b>\"{DisplayName}\"</b> action. Please consider assigning a <b>{TargetComponentType.Name}</b> component or removing the action.", target);
                    return null;
                }
            }

            originalText = targetText.text;

            TweenerCore<string, string, StringOptions> tween = targetText.DOText(text, duration, richText, scrambleMode);

            return tween;
        }

        protected override void ResetToInitialState_Internal()
        {
            if (targetText == null)
                return;

            if (string.IsNullOrEmpty(originalText))
                return;

            targetText.text = originalText;
        }
    }
}
#endif