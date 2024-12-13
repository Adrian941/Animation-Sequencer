#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
    [DisallowMultipleComponent]
    public class AnimationSequencerController : MonoBehaviour
    {
        #region Enumerations
        public enum PlayType
        {
            Forward,
            Backward
        }
        
        public enum AutoplayType
        {
            Nothing,
            Start,
            OnEnable
        }

        public enum KillType
        {
            None,
            Reset,
            Complete
        }
        #endregion

        #region Variables
        // Public properties
        public AnimationStepBase[] AnimationSteps { get { return animationSteps; } }
        public float PlaybackSpeed => playbackSpeed;
        public PlayType PlayTypeDirection => playType;
        public int Loops => loops;
        public UnityEvent OnAnimationStart { get { return onAnimationStart; } protected set { onAnimationStart = value; } }
        public UnityEvent OnAnimationProgress { get { return onAnimationProgress; } protected set { onAnimationProgress = value; } }
        public UnityEvent OnAnimationFinish { get { return onAnimationFinish; } protected set { onAnimationFinish = value; } }
        public Sequence PlayingSequence => playingSequence;
        public bool IsPlaying => playingSequence != null && playingSequence.IsPlaying();
        /// <summary>
        /// Extra interval added on "Callbacks" for a bug when this tween runs in "Backwards" direction.
        /// </summary>
        public float ExtraIntervalAdded { get { return extraIntervalAdded; } }

        // Serialized fields
        [SerializeReference]
        private AnimationStepBase[] animationSteps = Array.Empty<AnimationStepBase>();
        [SerializeField]
        private UpdateType updateType;
        [Tooltip("If true, the animation is independent of the Time Scale.")]
        [SerializeField]
        private bool timeScaleIndependent;
        [SerializeField]
        private AutoplayType autoplayMode = AutoplayType.Start;
        [SerializeField]
        protected bool startPaused;
        [SerializeField]
        private float playbackSpeed = 1f;
        [Tooltip("Direction of the animation (Forward or Backward).")]
        [SerializeField]
        protected PlayType playType;
        [Tooltip("Number of loops for the animation (0 for no loops).")]
        [SerializeField]
        private int loops;
        [SerializeField]
        private LoopType loopType;
        [Tooltip("If true, the animation is automatically killed (released) after completion, which is useful for animations that are occasional. " +
            "If false, the animation persists, ideal for frequently recurring animations.")]
        [SerializeField]
        private bool autoKill = true;

        // Serialized events
        [SerializeField]
        private UnityEvent onAnimationStart = new UnityEvent();
        [SerializeField]
        private UnityEvent onAnimationProgress = new UnityEvent();
        [SerializeField]
        private UnityEvent onAnimationFinish = new UnityEvent();

        // Private variables
        private Sequence playingSequence;
        private UnityAction onCompleteCallback;
        private bool isSequenceJustGenerated;
        private bool resetStateWhenCreateSequence;
        private float extraIntervalAdded;

#if UNITY_EDITOR
        // Editor-only variables
        private bool requiresReset;
#endif
        #endregion

        #region Unity lifecycle methods
        protected virtual void Start()
        {
            if (autoplayMode != AutoplayType.Start)
                return;

            Autoplay();
        }
        
        protected virtual void OnEnable()
        {
            if (autoplayMode != AutoplayType.OnEnable)
                return;

            Autoplay();
        }
        
        protected virtual void OnDisable()
        {
            if (playingSequence == null)
                return;

            ClearPlayingSequence();
        }

        protected virtual void OnDestroy()
        {
            ClearPlayingSequence();
        }
        #endregion

        #region Sequencer lifecycle methods
        #region Playback Control
        private void Autoplay()
        {
            Play();

            if (startPaused)
                playingSequence.Pause();
        }

        public virtual void Play()
        {
            Play(false, null);
        }

        public virtual void Play(bool resetFirst = false, UnityAction onCompleteCallback = null)
        {
            Play_Internal(playType, resetFirst, onCompleteCallback);
        }

        protected virtual void Play_Internal(PlayType playDirection, bool resetFirst = false, UnityAction onCompleteCallback = null)
        {
            if (!isActiveAndEnabled)
                return;

            this.onCompleteCallback = onCompleteCallback;

            //Create the sequence if it does not exist.
            if (playingSequence == null)
            {
                if (Application.isPlaying && resetStateWhenCreateSequence)
                    ResetToInitialState();

                playingSequence = GenerateSequence();
                isSequenceJustGenerated = true;
                resetStateWhenCreateSequence = true;
            }

            switch (playDirection)
            {
                case PlayType.Forward:
                    //Reset the animation if "resetFirst" = true or the sequence is complete.
                    if (resetFirst || playingSequence.fullPosition >= playingSequence.Duration())
                        playingSequence.Rewind();

                    playingSequence.PlayForward();
                    break;
                case PlayType.Backward:
                    //Reset the animation if "resetFirst" = true, the sequence has just been generated or the sequence is complete.
                    if (resetFirst || isSequenceJustGenerated || playingSequence.fullPosition <= 0f)
                        playingSequence.Goto(playingSequence.Duration());

                    playingSequence.PlayBackwards();
                    break;
            }

            isSequenceJustGenerated = false;
        }

        public virtual void PlayForward()
        {
            PlayForward(false, null);
        }

        public virtual void PlayForward(bool resetFirst = false, UnityAction onCompleteCallback = null)
        {
            Play_Internal(PlayType.Forward, resetFirst, onCompleteCallback);
        }

        public virtual void PlayBackwards()
        {
            PlayBackwards(false, null);
        }

        public virtual void PlayBackwards(bool completeFirst = false, UnityAction onCompleteCallback = null)
        {
            Play_Internal(PlayType.Backward, completeFirst, onCompleteCallback);
        }
        #endregion

        #region Time and Progress Management
        public virtual void Goto(float timePosition, bool WithCallbacks = true, bool andPlay = false)
        {
            if (playingSequence == null)
                Play();

            if (WithCallbacks)
                playingSequence.GotoWithCallbacks(timePosition, andPlay);
            else
                playingSequence.Goto(timePosition, andPlay);
        }
        
        public virtual void SetProgress(float progress, bool WithCallbacks = true, bool andPlay = false)
        {
            if (playingSequence == null)
                Play();

            if (playingSequence == null)
                return;

            progress = Mathf.Clamp01(progress);
            if (WithCallbacks)
                playingSequence.GotoWithCallbacks(progress * playingSequence.Duration(), andPlay);
            else
                playingSequence.Goto(progress * playingSequence.Duration(), andPlay);
        }
        #endregion

        #region Pause, Resume, and Complete
        public virtual void TogglePause()
        {
            if (playingSequence == null)
                return;

            playingSequence.TogglePause();
        }

        public virtual void Pause()
        {
            if (!IsPlaying)
                return;

            playingSequence.Pause();
        }

        public virtual void Resume()
        {
            if (playingSequence == null)
                return;

            playingSequence.Play();
        }

        /// <summary>
        /// Rewinds the sequence to its starting position.
        /// </summary>
        /// <param name="includeDelay"></param>
        public virtual void Rewind(bool includeDelay = true)
        {
            if (playingSequence == null)
                return;

            playingSequence.Rewind(includeDelay);
        }

        /// <summary>
        /// Rewinds the sequence based on its current play direction.
        /// For forward playback, it rewinds to the start. 
        /// For backward playback, it rewinds to the end of the sequence.
        /// </summary>
        /// <param name="includeDelay"></param>
        public virtual void RewindCurrentPlayDirection(bool includeDelay = true)
        {
            if (playingSequence == null)
                return;

            if (!playingSequence.IsBackwards())
                playingSequence.Rewind(includeDelay);
            else
                playingSequence.Goto(playingSequence.Duration());
        }

        /// <summary>
        /// Completes the sequence immediately, moving it to its final position.
        /// </summary>
        /// <param name="withCallbacks"></param>
        public virtual void Complete(bool withCallbacks = false)
        {
            if (playingSequence == null)
                return;

            playingSequence.Complete(withCallbacks);
        }

        /// <summary>
        /// Completes the sequence based on its current play direction.
        /// For forward playback, moves to the end of the sequence.
        /// For backward playback, moves to the start of the sequence.
        /// </summary>
        /// <param name="withCallbacks"></param>
        public virtual void CompleteCurrentPlayDirection(bool withCallbacks = false)
        {
            if (playingSequence == null)
                return;

            if (!playingSequence.IsBackwards())
            {
                playingSequence.Complete(withCallbacks);
            }
            else
            {
                if (withCallbacks)
                    playingSequence.GotoWithCallbacks(0);
                else
                    playingSequence.Goto(0);
            }
        }

        public virtual void Kill(KillType killType = KillType.Reset)
        {
            if (playingSequence == null)
                return;

            switch (killType)
            {
                case KillType.Reset:
                    SetProgress(0);
                    break;
                case KillType.Complete:
                    SetProgress(1);
                    break;
            }

            ClearPlayingSequence();
        }
        #endregion

        #region Sequence Generation and Reset
        public virtual Sequence GenerateSequence()
        {
            Sequence sequence = DOTween.Sequence();
            
            // Various edge cases exists with OnStart() and OnComplete(), some of which can be solved with OnRewind(),
            // but it still leaves callbacks unfired when reversing direction after natural completion of the animation.
            // Rather than using the in-built callbacks, we simply bookend the Sequence with AppendCallback to ensure
            // a Start and Finish callback is always fired.
            sequence.AppendCallback(() =>
            {
                if (!sequence.IsBackwards())
                    AnimationStarted();
                else
                    AnimationFinished();
            });

            extraIntervalAdded = 0;
            for (int i = 0; i < animationSteps.Length; i++)
            {
                AnimationStepBase animationStepBase = animationSteps[i];
                animationStepBase.AddTweenToSequence(sequence);
                extraIntervalAdded += animationStepBase.GetExtraIntervalAdded();
            }

            sequence.SetTarget(this);
            sequence.SetAutoKill(autoKill);
            sequence.SetUpdate(updateType, timeScaleIndependent);
            sequence.OnUpdate(() => onAnimationProgress.Invoke());
            sequence.OnKill(() => playingSequence = null);
            // See comment above regarding bookending via AppendCallback.
            sequence.AppendCallback(() =>
            {
                if (!sequence.IsBackwards())
                    AnimationFinished();
                else
                    AnimationStarted();
            });

            int targetLoops = loops;
            if (!Application.isPlaying)
            {
                if (loops == -1)
                {
                    targetLoops = 10;
                    Debug.LogWarning("Infinity sequences on editor can cause issues, using 10 loops while on editor.");
                }
            }
            sequence.SetLoops(targetLoops, loopType);
            sequence.timeScale = playbackSpeed;

            if (loops == -1)
                extraIntervalAdded = !Application.isPlaying ? extraIntervalAdded * targetLoops : extraIntervalAdded;
            else if (loops > 1)
                extraIntervalAdded *= loops;

            return sequence;
        }

        private void AnimationStarted()
        {
            onAnimationStart.Invoke();
        }

        private void AnimationFinished()
        {
            onAnimationFinish.Invoke();
            if (onCompleteCallback != null)
            {
                onCompleteCallback.Invoke();
                onCompleteCallback = null;
            }

            //Kill the sequence manually if autokill = true when "Backwards" sequence is completed.
            //The reason: DoTween does not kill the sequence even though kill = true only in the case of "Backwards".
            if (playingSequence != null && playingSequence.IsBackwards() && Application.isPlaying && autoKill)
                ClearPlayingSequence();
        }

        public virtual void ResetToInitialState()
        {
            for (int i = animationSteps.Length - 1; i >= 0; i--)
            {
                animationSteps[i].ResetToInitialState();
            }
        }

        public void ClearPlayingSequence()
        {
            DOTween.Kill(this);
            DOTween.Kill(playingSequence);
            playingSequence = null;
        }
        #endregion
        #endregion

        #region Set values
        public void SetAutoplayMode(AutoplayType autoplayType)
        {
            autoplayMode = autoplayType;
        }
        
        public void SetPauseOnStart(bool targetPauseOnAwake)
        {
            startPaused = targetPauseOnAwake;
        }
        
        public void SetTimeScaleIndependent(bool targetTimeScaleIndependent)
        {
            timeScaleIndependent = targetTimeScaleIndependent;
        }
        
        public void SetPlayType(PlayType targetPlayType)
        {
            playType = targetPlayType;
        }
        
        public void SetUpdateType(UpdateType targetUpdateType)
        {
            updateType = targetUpdateType;
        }
        
        public void SetAutoKill(bool targetAutoKill)
        {
            autoKill = targetAutoKill;
        }
        
        public void SetLoops(int targetLoops)
        {
            loops = targetLoops;
        }
        #endregion

        #region Editor-Only methods
#if UNITY_EDITOR
        // Unity Event Function called when component is added or reset.
        private void Reset()
        {
            requiresReset = true;
        }

        // Used by the CustomEditor so it knows when to reset to the defaults.
        public bool IsResetRequired()
        {
            return requiresReset;
        }

        // Called by the CustomEditor once the reset has been completed 
        public void ResetComplete()
        {
            requiresReset = false;
        }
#endif
        #endregion

        #region Helper methods
        public bool TryGetStepAtIndex<T>(int index, out T result) where T : AnimationStepBase
        {
            if (index < 0 || index > animationSteps.Length - 2)
            {
                result = null;
                return false;
            }

            result = animationSteps[index] as T;
            return result != null;
        }
        #endregion
    }
}
#endif