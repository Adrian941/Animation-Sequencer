#if DOTWEEN_ENABLED
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
    [CreateAssetMenu(menuName = "Animation Sequencer/Create Animation Sequencer Defaults", fileName = "AnimationSequencerDefaults")]
    public sealed class AnimationSequencerDefaults : EditorDefaultResourceSingleton<AnimationSequencerDefaults>
    {
        [Header("Animation Sequencer defaults")]
        //[Header("Defaults when a new instance class is created")]
        [SerializeField]
        private AutoplayType autoplayMode = AutoplayType.Start;
        public AutoplayType AutoplayMode => autoplayMode;
        
        [SerializeField]
        private bool startPaused;
        public bool StartPaused => startPaused;

        [SerializeField]
        private PlayType playType = PlayType.Forward;
        public PlayType PlayType => playType;

        [SerializeField]
        private UpdateType updateType = UpdateType.Normal;
        public UpdateType UpdateType => updateType;

        [SerializeField]
        private bool timeScaleIndependent;
        public bool TimeScaleIndependent => timeScaleIndependent;

        [SerializeField]
        private int loops;
        public int Loops => loops;

        [SerializeField]
        private bool autoKill = true;
        public bool AutoKill => autoKill;

        [Header("Actions defaults")]
        [SerializeField]
        private AnimationDirection direction = AnimationDirection.To;
        public AnimationDirection Direction => direction;

        [SerializeField]
        private CustomEase ease = CustomEase.InOutQuad;
        public CustomEase Ease => ease;

        [Header("Use Previous Actions values")]
        [SerializeField]
        private bool usePreviousDirection = true;
        public bool UsePreviousDirection => usePreviousDirection;

        [SerializeField]
        private bool usePreviousEase = true;
        public bool UsePreviousEase => usePreviousEase;
    }
}
#endif