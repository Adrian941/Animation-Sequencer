#if DOTWEEN_ENABLED
#if TMP_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class TMP_TextDOTweenAction : DOTweenActionBase
    {
        public override Type TargetComponentType => typeof(TMP_Text);
        public override string DisplayName => "TMP Text";

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

        private TMP_Text targetTmpText;
        private string originalText;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (targetTmpText == null || targetTmpText.gameObject != target)
            {
                targetTmpText = target.GetComponent<TMP_Text>();
                if (targetTmpText == null)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component");
                    return null;
                }
            }

            originalText = targetTmpText.text;

            TweenerCore<string, string, StringOptions> tween = targetTmpText.DOText(text, duration, richText, scrambleMode);

            return tween;
        }

        public override void ResetToInitialState()
        {
            if (targetTmpText == null)
                return;

            if (string.IsNullOrEmpty(originalText))
                return;

            targetTmpText.text = originalText;
        }
    }
}
#endif
#endif