#if DOTWEEN_ENABLED
using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
    [Serializable]
    public sealed class TweenAnimationStep : GameObjectAnimationStep
    {
        public override string DisplayName => "Tween Target";

        [Tooltip("Number of loops for the animation (0 for no loops).")]
        [SerializeField]
        private int loopCount;
        public int LoopCount
        {
            get => loopCount;
            set => loopCount = value;
        }

        [SerializeField]
        private LoopType loopType;
        public LoopType LoopType
        {
            get => loopType;
            set => loopType = value;
        }

        [SerializeReference]
        private TweenActionBase[] actions;
        public TweenActionBase[] Actions
        {
            get => actions;
            set => actions = value;
        }

        public override void AddTweenToSequence(Sequence animationSequence)
        {
            if (target == null)
            {
                Debug.LogWarning($"The <b>\"{DisplayName}\"</b> Step does not have a <b>\"Target\"</b>. Please consider assigning a <b>\"Target\"</b> or removing the step.");
                return;
            }

            Sequence sequence = DOTween.Sequence();
            bool isDelayAssigned = false;
            for (int i = 0; i < actions.Length; i++)
            {
                Tween tween = actions[i].GenerateTween(target, duration, this);
                if (tween == null)
                    continue;

                if (!isDelayAssigned)
                {
                    tween.SetDelay(Delay);
                    isDelayAssigned = true;
                }
                sequence.Join(tween);
            }

            sequence.SetLoops(loopCount, loopType);

            if (FlowType == FlowType.Join)
                animationSequence.Join(sequence);
            else
                animationSequence.Append(sequence);

            RefreshEditorOnSequenceUpdate(sequence, target.transform);
        }

        public override void ResetToInitialState()
        {
            if (target == null)
                return;

            for (int i = actions.Length - 1; i >= 0; i--)
            {
                actions[i].ResetToInitialState();
            }

            if (target != null)
                RefreshEditor(target.transform);
        }

        public override string GetDisplayNameForEditor(int index)
        {
            string targetName = "NULL";
            if (target != null)
                targetName = target.name;

            return $"{index}. Tween \"{targetName}\": {string.Join(", ", actions.Select(action => action.DisplayName))}";
        }

        public override float GetDuration()
        {
            if (loopCount > 1)
                return base.GetDuration() + duration * loopCount;

            else
                return base.GetDuration() + duration;
        }

        public bool TryGetActionAtIndex<T>(int index, out T result) where T : TweenActionBase
        {
            if (index < 0 || index > actions.Length - 2)
            {
                result = null;
                return false;
            }

            result = actions[index] as T;
            return result != null;
        }

        private void RefreshEditorOnSequenceUpdate(Sequence sequence, Transform targetTransform)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                sequence.OnUpdate(() =>
                {
                    RefreshEditor(targetTransform);
                });
            }
#endif
        }

#if UNITY_EDITOR
        private VersionComparator.VersionComparisonResult versionComparison = VersionComparator.VersionComparisonResult.IncorrectFormat;
#endif

        // Work around a Unity bug where updating some UI properties like the colour does not cause any visual change outside of PlayMode.
        // https://forum.unity.com/threads/editor-scripting-force-color-update.798663/
        // The bug was fixed from DoTween version "1.2.735". I keep this patch because I don't like DoTween marking the scene as dirty.
        // https://github.com/Demigiant/dotween/commit/89607add6d8fc275cb000d0f67769df55adffc5b
        private void RefreshEditor(Transform targetTransform)
        {
#if UNITY_EDITOR
            if (versionComparison == VersionComparator.VersionComparisonResult.IncorrectFormat)
                versionComparison = VersionComparator.Compare(DOTween.Version, "1.2.705");

            if (!Application.isPlaying && targetTransform != null && versionComparison <= VersionComparator.VersionComparisonResult.Equal)
            {
                if (targetTransform.hasChanged)
                {
                    targetTransform.hasChanged = false;
                    return;
                }

                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            }
#endif
        }
    }
}
#endif