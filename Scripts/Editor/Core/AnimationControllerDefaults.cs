#if DOTWEEN_ENABLED
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
    [CreateAssetMenu(menuName = "Animation Sequencer/Create Animation Sequencer Default", fileName = "AnimationControllerDefaults")]
    public sealed class AnimationControllerDefaults : EditorDefaultResourceSingleton<AnimationControllerDefaults>
    {
        [Header("Animation Sequencer defaults")]
        [SerializeField]
        private AnimationSequencerController.AutoplayType autoplayMode = AnimationSequencerController.AutoplayType.Start;
        public AnimationSequencerController.AutoplayType AutoplayMode => autoplayMode;
        
        [SerializeField]
        private bool startPaused = false;
        public bool StartPaused => startPaused;

        [SerializeField]
        private AnimationSequencerController.PlayType playType = AnimationSequencerController.PlayType.Forward;
        public AnimationSequencerController.PlayType PlayType => playType;

        [SerializeField]
        private UpdateType updateType = UpdateType.Normal;
        public UpdateType UpdateType => updateType;

        [SerializeField]
        private bool timeScaleIndependent = false;
        public bool TimeScaleIndependent => timeScaleIndependent;

        [SerializeField]
        private int loops = 0;
        public int Loops => loops;

        [SerializeField]
        private bool autoKill = true;
        public bool AutoKill => autoKill;

        [Header("Actions defaults")]
        [SerializeField]
        private TweenActionBase.AnimationDirection direction = TweenActionBase.AnimationDirection.To;
        public TweenActionBase.AnimationDirection Direction => direction;

        [SerializeField]
        private CustomEase ease = CustomEase.InOutQuad;
        public CustomEase Ease => ease;

        [SerializeField]
        private bool relative = false;
        public bool Relative => relative;

        [Header("Use Previous Actions values")]
        [SerializeField]
        private bool usePreviousDirection = true;
        public bool UsePreviousDirection => usePreviousDirection;

        [SerializeField]
        private bool usePreviousEase = true;
        public bool UsePreviousEase => usePreviousEase;

        [SerializeField]
        private bool usePreviousRelative = true;
        public bool UsePreviousRelative => usePreviousRelative;
    }
}
#endif