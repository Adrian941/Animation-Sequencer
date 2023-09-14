#if DOTWEEN_ENABLED
using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrunoMikoski.AnimationSequencer
{
    [DisallowMultipleComponent]
    public class AnimationSequencerController : MonoBehaviour
    {
        public enum PlayType
        {
            Forward,
            Backward
        }
        
        public enum AutoplayType
        {
            Start,
            OnEnable,
            Nothing
        }

        public enum KillType
        {
            None,
            Reset,
            Complete
        }

        [SerializeReference]
        private AnimationStepBase[] animationSteps = Array.Empty<AnimationStepBase>();
        [SerializeField]
        private UpdateType updateType = UpdateType.Normal;
        [SerializeField]
        private bool timeScaleIndependent = false;
        [SerializeField]
        private AutoplayType autoplayMode = AutoplayType.Start;
        [SerializeField]
        protected bool startPaused;
        [SerializeField]
        private float playbackSpeed = 1f;
        public float PlaybackSpeed => playbackSpeed;
        [SerializeField]
        protected PlayType playType = PlayType.Forward;
        [SerializeField]
        private int loops = 0;
        [SerializeField]
        private LoopType loopType = LoopType.Restart;
        [SerializeField]
        private bool autoKill = true;
        
        [SerializeField]
        private UnityEvent onStartEvent = new UnityEvent();
        public UnityEvent OnStartEvent { get { return onStartEvent;} protected set {onStartEvent = value;}}
        [SerializeField]
        private UnityEvent onFinishedEvent = new UnityEvent();
        public UnityEvent OnFinishedEvent { get { return onFinishedEvent;} protected set {onFinishedEvent = value;}}
        [SerializeField]
        private UnityEvent onProgressEvent = new UnityEvent();
        public UnityEvent OnProgressEvent => onProgressEvent;

        private Sequence playingSequence;
        public Sequence PlayingSequence => playingSequence;
        private PlayType playTypeInternal = PlayType.Forward;
#if UNITY_EDITOR
        private bool requiresReset = false;
#endif
        public bool IsPlaying => playingSequence != null && playingSequence.IsActive() && playingSequence.IsPlaying();
        public bool IsPaused => playingSequence != null && playingSequence.IsActive() && !playingSequence.IsPlaying();

        private bool isSequenceGenerated;
        private bool resetWhenCreateSequence;


        protected virtual void Awake()
        {
            playTypeInternal = playType;
        }

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

        private void Autoplay()
        {
            Play();
            if (startPaused)
                playingSequence.Pause();
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

        public virtual void Play()
        {
            Play(false, null);
        }

        public virtual void Play(bool resetFirst = false, Action onCompleteCallback = null)
        {
            //In editor mode, always take the "PlayType" assigned in the inspector.
            if (!Application.isPlaying)
                playTypeInternal = playType;

            //"Backwards" does not work with "Loops", so play the "Forward" sequence.
            if (playTypeInternal == PlayType.Backward && loops != 0)
                playTypeInternal = PlayType.Forward;

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
                case PlayType.Backward:
                    //Reset the animation if "resetFirst" = true, the sequence has just been generated or the sequence is complete.
                    if (resetFirst || isSequenceGenerated || (!playingSequence.IsComplete() && !IsPlaying))
                        playingSequence.Goto(playingSequence.Duration());

                    playingSequence.PlayBackwards();
                    break;
                case PlayType.Forward:
                    //Reset the animation if "resetFirst" = true or the sequence is complete.
                    if (resetFirst || (!autoKill && playingSequence.IsComplete()))
                        playingSequence.Goto(0);

                    playingSequence.PlayForward();
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

            Play(resetFirst, onCompleteCallback);
        }

        public virtual void PlayBackwards()
        {
            PlayBackwards(false, null);
        }

        public virtual void PlayBackwards(bool completeFirst = false, Action onCompleteCallback = null)
        {
            playTypeInternal = PlayType.Backward;

            Play(completeFirst, onCompleteCallback);
        }

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

        public virtual IEnumerator PlayEnumerator()
        {
            Play();
            yield return playingSequence.WaitForCompletion();
        }

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
            
            for (int i = 0; i < animationSteps.Length; i++)
            {
                animationSteps[i].AddTweenToSequence(sequence);
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
  
        public void SetAutoplayMode(AutoplayType autoplayType)
        {
            autoplayMode = autoplayType;
        }
        
        public void SetPlayOnStart(bool targetPlayOnAwake)
        {
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

        // Unity Event Function called when component is added or modified.
        private void OnValidate()
        {
            CalculateStepsDuration();
        }
#endif
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

        /// <summary>
        /// Called by the Editor to calculate the main sequence duration and "StartTime" of each step relative to the main sequence.
        /// </summary>
        private void CalculateStepsDuration()
        {
            //Calculate the main sequence duration and "StartTime" of each step.
            float mainSequenceDuration = 0;
            float[] startTimeSteps = new float[animationSteps.Length];
            AnimationStepBase longestDurationStep = null;

            for (int i = 0; i < animationSteps.Length; i++)
            {
                AnimationStepBase step = animationSteps[i];
                startTimeSteps[i] = mainSequenceDuration;

                if (i == 0 || step.FlowType == FlowType.Append || step.GetDuration() > longestDurationStep.GetDuration())
                    longestDurationStep = step;

                int nextStepIndex = i + 1;
                if (nextStepIndex >= animationSteps.Length || animationSteps[nextStepIndex].FlowType == FlowType.Append)
                    mainSequenceDuration += longestDurationStep.GetDuration();
            }

            if (loops > 1)
                mainSequenceDuration *= loops;

            //Assign the main sequence duration and "StartTime" to each step. 
            for (int i = 0; i < animationSteps.Length; i++)
            {
                AnimationStepBase step = animationSteps[i];
                step.SetAnimationData(mainSequenceDuration, startTimeSteps[i]);
            }
        }
    }
}
#endif