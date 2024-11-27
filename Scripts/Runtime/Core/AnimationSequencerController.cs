#if DOTWEEN_ENABLED
using System;
using System.Collections;
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
        public UnityEvent OnStartEvent { get { return onStartEvent; } protected set { onStartEvent = value; } }
        public UnityEvent OnProgressEvent { get { return onProgressEvent; } protected set { onProgressEvent = value; } }
        public UnityEvent OnFinishedEvent { get { return onFinishedEvent; } protected set { onFinishedEvent = value; } }
        public Sequence PlayingSequence => playingSequence;
        public bool IsPlaying => playingSequence != null && playingSequence.IsActive() && playingSequence.IsPlaying();
        /// <summary>
        /// Extra interval added on "Callbacks" for a bug when this tween runs in "Backwards" direction.
        /// </summary>
        public float ExtraIntervalAdded { get { return extraIntervalAdded; } }

        // Serialized fields
        [SerializeReference]
        private AnimationStepBase[] animationSteps = Array.Empty<AnimationStepBase>();
        [SerializeField]
        private UpdateType updateType = UpdateType.Normal;
        [Tooltip("If true, the animation is independent of the Time Scale.")]
        [SerializeField]
        private bool timeScaleIndependent = false;
        [SerializeField]
        private AutoplayType autoplayMode = AutoplayType.Start;
        [SerializeField]
        protected bool startPaused;
        [SerializeField]
        private float playbackSpeed = 1f;
        [Tooltip("Direction of the animation (Forward or Backward).")]
        [SerializeField]
        protected PlayType playType = PlayType.Forward;
        [Tooltip("Number of loops for the animation (0 for no loops).")]
        [SerializeField]
        private int loops = 0;
        [SerializeField]
        private LoopType loopType = LoopType.Restart;
        [Tooltip("If true, the animation is automatically killed (released) after completion, which is useful for animations that are occasional. " +
            "If false, the animation persists, ideal for frequently recurring animations.")]
        [SerializeField]
        private bool autoKill = true;

        // Serialized events
        [SerializeField]
        private UnityEvent onStartEvent = new UnityEvent();
        [SerializeField]
        private UnityEvent onProgressEvent = new UnityEvent();
        [SerializeField]
        private UnityEvent onFinishedEvent = new UnityEvent();

        // Private variables
        private Sequence playingSequence;       
        private PlayType playTypeInternal = PlayType.Forward;       
        private bool isSequenceGenerated;
        private bool resetWhenCreateSequence;
        private float extraIntervalAdded;

#if UNITY_EDITOR
        // Editor-only variables
        private bool requiresReset = false;
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
            if (autoplayMode != AutoplayType.OnEnable)
                return;
            
            if (playingSequence == null)
                return;

            ClearPlayingSequence();
            // Reset the object to its initial state so that if it is re-enabled the start values are correct for
            // regenerating the Sequence.
            ResetToInitialState();
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

        public virtual void Play(bool resetFirst = false, Action onCompleteCallback = null)
        {
            playTypeInternal = playType;

            Play_Internal(resetFirst, onCompleteCallback);
        }

        protected virtual void Play_Internal(bool resetFirst = false, Action onCompleteCallback = null)
        {
            //Clean and assign the "OnFinished" event.
            onFinishedEvent.RemoveAllListeners();
            if (onCompleteCallback != null)
                onFinishedEvent.AddListener(onCompleteCallback.Invoke);

            //Create the sequence if it does not exist.
            if (playingSequence == null)
            {
                if (Application.isPlaying && autoKill && resetWhenCreateSequence)
                    ResetToInitialState();

                playingSequence = GenerateSequence();
                isSequenceGenerated = true;
                resetWhenCreateSequence = true;
            }

            switch (playTypeInternal)
            {
                case PlayType.Forward:
                    //Reset the animation if "resetFirst" = true or the sequence is complete.
                    if (resetFirst || (!autoKill && playingSequence.IsComplete()))
                        playingSequence.Goto(0);

                    playingSequence.PlayForward();
                    break;
                case PlayType.Backward:
                    //Reset the animation if "resetFirst" = true, the sequence has just been generated or the sequence is complete.
                    if (resetFirst || isSequenceGenerated || (!autoKill && !playingSequence.IsComplete() && !IsPlaying))
                        playingSequence.Goto(playingSequence.Duration());

                    playingSequence.PlayBackwards();
                    break;
                default:
                    playingSequence.Play();
                    break;
            }

            isSequenceGenerated = false;
        }

        public virtual void PlayForward()
        {
            PlayForward(false, null);
        }

        public virtual void PlayForward(bool resetFirst = false, Action onCompleteCallback = null)
        {
            playTypeInternal = PlayType.Forward;

            Play_Internal(resetFirst, onCompleteCallback);
        }

        public virtual void PlayBackwards()
        {
            PlayBackwards(false, null);
        }

        public virtual void PlayBackwards(bool completeFirst = false, Action onCompleteCallback = null)
        {
            playTypeInternal = PlayType.Backward;

            Play_Internal(completeFirst, onCompleteCallback);
        }

        public virtual IEnumerator PlayEnumerator()
        {
            Play();
            yield return playingSequence.WaitForCompletion();
        }
        #endregion

        #region Time and Progress Management
        public virtual void SetTime(float seconds, bool andPlay = true)
        {
            if (playingSequence == null)
                Play();

            float duration = playingSequence.Duration();
            float progress = Mathf.Clamp01(seconds / duration);
            SetProgress(progress, andPlay);
        }
        
        public virtual void SetProgress(float progress, bool andPlay = true)
        {
            progress = Mathf.Clamp01(progress);
            
            if (playingSequence == null)
                Play();

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

        public virtual void Complete(bool withCallbacks = true)
        {
            if (playingSequence == null)
                return;

            playingSequence.Complete(withCallbacks);
        }

        public virtual void Rewind(bool includeDelay = true)
        {
            if (playingSequence == null)
                return;

            playingSequence.Rewind(includeDelay);
        }

        public virtual void Kill(KillType killType = KillType.Reset)
        {
            DOTween.Kill(this, killType == KillType.Complete);
            DOTween.Kill(playingSequence, killType == KillType.Complete);

            if (killType == KillType.Reset)
                ResetToInitialState();

            playingSequence = null;
            resetWhenCreateSequence = false;
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
                if (playTypeInternal == PlayType.Forward)
                {
                    onStartEvent.Invoke();
                }
                else
                {
                    onFinishedEvent.Invoke();

                    //Kill the sequence manually if autokill = true when "Backwards" sequence is completed.
                    //The reason: DoTween does not kill the sequence even though kill = true only in the case of "Backwards".
                    if (Application.isPlaying && autoKill)
                        ClearPlayingSequence();
                }
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
            sequence.OnUpdate(() => onProgressEvent.Invoke());
            sequence.OnKill(() => playingSequence = null);
            // See comment above regarding bookending via AppendCallback.
            sequence.AppendCallback(() =>
            {
                if (playTypeInternal == PlayType.Forward)
                    onFinishedEvent.Invoke();
                else
                    onStartEvent.Invoke();
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
            //sequence.SetDelay(-extraIntervalAdded); //Remove extra interval added on "Callbacks" for a bug when this tween runs in "Backwards" direction.

            if (loops == -1)
                extraIntervalAdded = !Application.isPlaying ? extraIntervalAdded * targetLoops : extraIntervalAdded;
            else if (loops > 1)
                extraIntervalAdded *= loops;

            return sequence;
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