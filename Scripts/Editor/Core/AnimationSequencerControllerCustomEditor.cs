#if DOTWEEN_ENABLED
using System;
using System.Collections.Generic;
using System.Reflection;
using DG.DOTweenEditor;
using DG.Tweening;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Modified by Pablo Huaxteco
    [CustomEditor(typeof(AnimationSequencerController), true)]
    public class AnimationSequencerControllerCustomEditor : Editor
    {
        #region Variables
        // Static variables and properties 
        private static AnimationStepAdvancedDropdown cachedAnimationStepsDropdown;
        private static AnimationStepAdvancedDropdown AnimationStepAdvancedDropdown
        {
            get
            {
                if (cachedAnimationStepsDropdown == null)
                    cachedAnimationStepsDropdown = new AnimationStepAdvancedDropdown(new AdvancedDropdownState());

                return cachedAnimationStepsDropdown;
            }
        }

        // Private variables
        private AnimationSequencerController sequencerController;
        private ReorderableList reorderableList;
        private GUIStyle topRightTextStyle;
        private StepAnimationData[] stepsAnimationData;
        private Dictionary<int, float> collapsedStepValues = new Dictionary<int, float>();
        private Dictionary<int, List<bool>> expandedActionStates = new Dictionary<int, List<bool>>();
        private SerializedProperty playbackSpeedProperty;
        private SerializedProperty autoPlayModeSerializedProperty;
        private SerializedProperty autoKillSerializedProperty;
        private MethodInfo reorderableListClearCacheMethod = null;
        private bool cantFindReorderableListClearCacheMethod;
        private bool showPreviewPanel = true;
        private bool showSettingsPanel;
        private bool showCallbacksPanel;
        private bool showStepsPanel;
        private float tweenTimeScale = 1f;
        private bool wasShowingStepsPanel;
        private bool justStartPreviewing;
        private float mainSequenceDuration = 0;
        private bool actionsValuesTaken = false;
        private int lastExpandedStepIndex = -1;
        private bool lastExpandedStepChanged;
        #endregion

        #region OnEnable/OnDisable settings
        private void OnEnable()
        {
            InitializeReferences();
            InitializeReorderableList();
            SubscribeToEditorEvents();
            Repaint();
        }

        private void OnDisable()
        {
            UnsubscribeFromEditorEvents();
            ResetEditorState();
        }

        private void InitializeReferences()
        {
            sequencerController = target as AnimationSequencerController;
            playbackSpeedProperty = serializedObject.FindProperty("playbackSpeed");
            autoPlayModeSerializedProperty = serializedObject.FindProperty("autoplayMode");
            autoKillSerializedProperty = serializedObject.FindProperty("autoKill");
        }

        private void InitializeReorderableList()
        {
            reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("animationSteps"), true, false, true, true);
            reorderableList.drawElementBackgroundCallback += OnDrawAnimationStepBackground;
            reorderableList.drawElementCallback += OnDrawAnimationStep;
            reorderableList.elementHeightCallback += GetAnimationStepHeight;
            reorderableList.onAddDropdownCallback += OnClickToAddNewAnimationStep;
            reorderableList.onRemoveCallback += OnClickToRemoveAnimationStep;
            reorderableList.onReorderCallbackWithDetails += OnAnimationStepListOrderChanged;
            reorderableList.onMouseDragCallback += OnMouseDragAnimationStep;
            reorderableList.onMouseUpCallback += OnMouseUpAnimationStep;
        }

        private void SubscribeToEditorEvents()
        {
            EditorApplication.playModeStateChanged += OnEditorPlayModeChanged;

#if UNITY_2021_1_OR_NEWER
            UnityEditor.SceneManagement.PrefabStage.prefabSaving += PrefabSaving;
#else
            UnityEditor.Experimental.SceneManagement.PrefabStage.prefabSaving += PrefabSaving;
#endif
        }

        private void UnsubscribeFromEditorEvents()
        {
            //Animation step reorderableList events.
            reorderableList.drawElementBackgroundCallback -= OnDrawAnimationStepBackground;
            reorderableList.drawElementCallback -= OnDrawAnimationStep;
            reorderableList.elementHeightCallback -= GetAnimationStepHeight;
            reorderableList.onAddDropdownCallback -= OnClickToAddNewAnimationStep;
            reorderableList.onRemoveCallback -= OnClickToRemoveAnimationStep;
            reorderableList.onReorderCallbackWithDetails -= OnAnimationStepListOrderChanged;
            reorderableList.onMouseDragCallback -= OnMouseDragAnimationStep;
            reorderableList.onMouseUpCallback -= OnMouseUpAnimationStep;

            //Other events.
            EditorApplication.playModeStateChanged -= OnEditorPlayModeChanged;

#if UNITY_2021_1_OR_NEWER
            UnityEditor.SceneManagement.PrefabStage.prefabSaving -= PrefabSaving;
#else
            UnityEditor.Experimental.SceneManagement.PrefabStage.prefabSaving -= PrefabSaving;
#endif
        }

        private void ResetEditorState()
        {
            if (!Application.isPlaying)
                StopSequence();

            tweenTimeScale = 1f;
            SerializedPropertyExtensions.ClearPropertyCache();
        }
        #endregion

        #region CustomEditor methods
        // Used to update the progress bar in the editor.
        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying || DOTweenEditorPreview.isPreviewing;
        }

        public override void OnInspectorGUI()
        {
            //Defaults.
            if (sequencerController.IsResetRequired())
                SetDefaults();

            //Styles.
            InitializeStyles();

            //Foldout areas.
            DrawFoldoutArea("Preview", ref showPreviewPanel, DrawPreviewControls, DrawExtraPreviewHeader);
            DrawFoldoutArea("Settings", ref showSettingsPanel, DrawSettings, DrawExtraSettingsHeader);
            DrawFoldoutArea("Callbacks", ref showCallbacksPanel, DrawCallbacks);
            SerializedProperty animationStepsProperty = null;
            if (!DOTweenEditorPreview.isPreviewing)
            {
                animationStepsProperty = reorderableList.serializedProperty;
                showStepsPanel = animationStepsProperty.isExpanded;
            }
            DrawFoldoutArea("Steps", ref showStepsPanel, DrawAnimationSteps);
            if (animationStepsProperty != null && !DOTweenEditorPreview.isPreviewing)
                animationStepsProperty.isExpanded = showStepsPanel;

            // Verify only one step is expanded.
            if (AnimationSequencerSettings.GetInstance().OnlyOneStepExpandedWhileEditing && showStepsPanel && !DOTweenEditorPreview.isPreviewing)
                CheckOnlyOneStepExpanded();
        }

        private void InitializeStyles()
        {
            if (topRightTextStyle == null)
            {
                topRightTextStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    normal = { textColor = new Color(0.1f, 0.1f, 0.1f) }
                };
            }
        }
        #endregion

        #region Sequencer defaults
        private void SetDefaults()
        {
            sequencerController = target as AnimationSequencerController;

            if (sequencerController != null)
            {
                sequencerController.SetAutoplayMode(AnimationControllerDefaults.Instance.AutoplayMode);
                sequencerController.SetPauseOnStart(AnimationControllerDefaults.Instance.StartPaused);
                sequencerController.SetTimeScaleIndependent(AnimationControllerDefaults.Instance.TimeScaleIndependent);
                sequencerController.SetPlayType(AnimationControllerDefaults.Instance.PlayType);
                sequencerController.SetUpdateType(AnimationControllerDefaults.Instance.UpdateType);
                sequencerController.SetAutoKill(AnimationControllerDefaults.Instance.AutoKill);
                sequencerController.SetLoops(AnimationControllerDefaults.Instance.Loops);
                sequencerController.ResetComplete();
            }
        }
        #endregion

        #region Preview panel
        private void DrawExtraPreviewHeader(Rect rect)
        {
            //Draw sequence duration.
            if (sequencerController.PlayingSequence != null && sequencerController.PlayingSequence.IsActive())
            {
                float duration = sequencerController.PlayingSequence.Duration() * (1 / playbackSpeedProperty.floatValue);
                DrawTopRightText(rect, $"Duration: {NumberFormatter.FormatDecimalPlaces(duration)}s", new Color(0f, 1f, 0f, 0.5f));
            }
        }

        private Rect DrawTopRightText(Rect rect, string text, Color color)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Vector2 textSize = topRightTextStyle.CalcSize(new GUIContent(text));
                Rect backgroundRect = new Rect(rect.x + rect.width - textSize.x - 4,
                    rect.y + 2,
                    textSize.x + 4,
                    textSize.y);
                EditorGUI.DrawRect(backgroundRect, color);
                GUI.Label(new Rect(backgroundRect.x + 2, backgroundRect.y, textSize.x, textSize.y), text, topRightTextStyle);

                rect.width -= textSize.x + 6;
            }

            return rect;
        }

        private void DrawPreviewControls()
        {
            DrawMediaPlayerControlButtons();
            DrawTimeScaleSlider();
            DrawProgressSlider();
            //DrawDurationInfo();
        }

        private void DrawMediaPlayerControlButtons()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool guiEnabled = GUI.enabled;

            GUIStyle previewButtonStyle = new GUIStyle(GUI.skin.button);
            previewButtonStyle.fixedWidth = previewButtonStyle.fixedHeight = 36;

            if (GUILayout.Button(AnimationSequenceEditorGUIUtility.BackButtonGUIContent, previewButtonStyle))
            {
                if (!sequencerController.IsPlaying)
                    PlaySequence();

                sequencerController.Rewind();
            }

            if (GUILayout.Button(AnimationSequenceEditorGUIUtility.StepBackGUIContent, previewButtonStyle))
            {
                if (!sequencerController.IsPlaying)
                    PlaySequence();

                StepBack();
            }

            if (sequencerController.IsPlaying)
            {
                if (GUILayout.Button(AnimationSequenceEditorGUIUtility.PauseButtonGUIContent, previewButtonStyle))
                    sequencerController.Pause();
            }
            else
            {
                if (GUILayout.Button(AnimationSequenceEditorGUIUtility.PlayButtonGUIContent, previewButtonStyle))
                    PlaySequence();
            }


            if (GUILayout.Button(AnimationSequenceEditorGUIUtility.StepNextGUIContent, previewButtonStyle))
            {
                if (!sequencerController.IsPlaying)
                    PlaySequence();

                StepNext();
            }

            if (GUILayout.Button(AnimationSequenceEditorGUIUtility.ForwardButtonGUIContent, previewButtonStyle))
            {
                if (!sequencerController.IsPlaying)
                    PlaySequence();

                sequencerController.Complete();
            }

            if (!Application.isPlaying)
            {
                GUI.enabled = DOTweenEditorPreview.isPreviewing;
                if (GUILayout.Button(AnimationSequenceEditorGUIUtility.StopButtonGUIContent, previewButtonStyle))
                {
                    StopSequence();
                }
            }

            GUI.enabled = guiEnabled;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void StepBack()
        {
            if (!sequencerController.IsPlaying)
                PlaySequence();

            sequencerController.PlayingSequence.Goto((sequencerController.PlayingSequence.ElapsedPercentage() -
                                                      0.01f) * sequencerController.PlayingSequence.Duration());
        }

        private void StepNext()
        {
            if (!sequencerController.IsPlaying)
                PlaySequence();

            sequencerController.PlayingSequence.Goto((sequencerController.PlayingSequence.ElapsedPercentage() +
                                                      0.01f) * sequencerController.PlayingSequence.Duration());
        }

        private void PlaySequence()
        {
            justStartPreviewing = false;

            if (!Application.isPlaying)
            {
                if (!DOTweenEditorPreview.isPreviewing)
                {
                    justStartPreviewing = true;
                    DOTweenEditorPreview.Start();

                    sequencerController.Play();

                    if (AnimationSequencerSettings.GetInstance().VisualizeStepsProgressWhenPreviewing)
                        CalculateStepsAnimationData();

                    if (AnimationSequencerSettings.GetInstance().CollapseStepsWhenPreviewing)
                        CollapseSteps();

                    DOTweenEditorPreview.PrepareTweenForPreview(sequencerController.PlayingSequence);
                }
                else
                {
                    if (sequencerController.PlayingSequence == null)
                    {
                        sequencerController.Play();
                    }
                    else
                    {
                        if (!sequencerController.PlayingSequence.IsBackwards() &&
                            sequencerController.PlayingSequence.fullPosition >= sequencerController.PlayingSequence.Duration())
                        {
                            sequencerController.Rewind();
                        }
                        else if (sequencerController.PlayingSequence.IsBackwards() &&
                                 sequencerController.PlayingSequence.fullPosition <= 0f)
                        {
                            sequencerController.Complete();
                        }

                        sequencerController.TogglePause();
                    }
                }
            }
            else
            {
                if (sequencerController.PlayingSequence == null)
                {
                    sequencerController.Play();
                }
                else
                {
                    if (sequencerController.PlayingSequence.IsActive())
                        sequencerController.TogglePause();
                    else
                        sequencerController.Play();
                }
            }

            if (justStartPreviewing)
                wasShowingStepsPanel = showStepsPanel;

            if (AnimationSequencerSettings.GetInstance().HideStepsWhenPreviewing)
                showStepsPanel = false;
        }

        private void StopSequence()
        {
            if (DOTweenEditorPreview.isPreviewing)
            {
                // Reset sequencer state.
                sequencerController.ResetToInitialState();
                sequencerController.ClearPlayingSequence();
                DOTweenEditorPreview.Stop();

                // Reset steps state.
                if (AnimationSequencerSettings.GetInstance().HideStepsWhenPreviewing)
                    showStepsPanel = wasShowingStepsPanel;

                if (AnimationSequencerSettings.GetInstance().CollapseStepsWhenPreviewing)
                    ExpandCollapsedSteps();
            }
        }

        private void DrawTimeScaleSlider()
        {
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();

            float normalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 65;
            tweenTimeScale = EditorGUILayout.Slider("TimeScale", tweenTimeScale, 0, 2);
            EditorGUIUtility.labelWidth = normalLabelWidth;

            UpdateSequenceTimeScale();

            GUILayout.FlexibleSpace();
        }

        private void DrawProgressSlider()
        {
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();

            float tweenProgress = GetCurrentSequencerProgress();

            float normalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 65;
            tweenProgress = EditorGUILayout.Slider("Progress", tweenProgress, 0, 1);
            EditorGUIUtility.labelWidth = normalLabelWidth;

            if (EditorGUI.EndChangeCheck())
                SetProgress(tweenProgress);

            GUILayout.FlexibleSpace();
        }

        private void SetProgress(float tweenProgress)
        {
            if (!sequencerController.IsPlaying)
                PlaySequence();

            sequencerController.PlayingSequence.Goto(tweenProgress * sequencerController.PlayingSequence.Duration());
        }

        private float GetCurrentSequencerProgress()
        {
            float tweenProgress;
            if (sequencerController.PlayingSequence != null && sequencerController.PlayingSequence.IsActive())
                tweenProgress = sequencerController.PlayingSequence.ElapsedPercentage();
            else
                tweenProgress = 0;

            return tweenProgress;
        }

        private void DrawDurationInfo()
        {
            if (sequencerController.PlayingSequence != null && sequencerController.PlayingSequence.IsActive())
                EditorGUILayout.HelpBox($"Sequence duration: {sequencerController.PlayingSequence.Duration()} seconds.", MessageType.Info);
        }
        #endregion

        #region Settings panel
        private void DrawExtraSettingsHeader(Rect rect)
        {
            //Draw auto kill value.
            rect = DrawTopRightText(rect, $"Auto kill: {autoKillSerializedProperty.boolValue}", new Color(1f, 0.2f, 0f, 0.5f));

            //Draw auto play mode.
            var autoplayMode = (AnimationSequencerController.AutoplayType)autoPlayModeSerializedProperty.enumValueIndex;
            string label = "";
            switch (autoplayMode)
            {
                case AnimationSequencerController.AutoplayType.Nothing:
                    label = "Off";
                    break;
                case AnimationSequencerController.AutoplayType.Start:
                    label = "on Start";
                    break;
                case AnimationSequencerController.AutoplayType.OnEnable:
                    label = "on Enable";
                    break;
            }

            DrawTopRightText(rect, $"Autoplay {label}", new Color(1f, 0.7f, 0f, 0.5f));
        }

        private void DrawSettings()
        {
            bool wasEnabled = GUI.enabled;
            if (DOTweenEditorPreview.isPreviewing)
                GUI.enabled = false;

            SerializedProperty pauseOnStartSerializedProperty = serializedObject.FindProperty("startPaused");
            SerializedProperty timeScaleIndependentSerializedProperty = serializedObject.FindProperty("timeScaleIndependent");
            SerializedProperty sequenceDirectionSerializedProperty = serializedObject.FindProperty("playType");
            SerializedProperty updateTypeSerializedProperty = serializedObject.FindProperty("updateType");
            SerializedProperty loopsSerializedProperty = serializedObject.FindProperty("loops");
            SerializedProperty loopTypeSerializedProperty = serializedObject.FindProperty("loopType");

            using (EditorGUI.ChangeCheckScope changedCheck = new EditorGUI.ChangeCheckScope())
            {
                AnimationSequencerController.AutoplayType autoplayMode = (AnimationSequencerController.AutoplayType)autoPlayModeSerializedProperty.enumValueIndex;
                EditorGUILayout.PropertyField(autoPlayModeSerializedProperty);
                if (autoplayMode != AnimationSequencerController.AutoplayType.Nothing)
                    EditorGUILayout.PropertyField(pauseOnStartSerializedProperty);
                EditorGUILayout.PropertyField(sequenceDirectionSerializedProperty);
                EditorGUILayout.PropertyField(updateTypeSerializedProperty);
                DrawPlaybackSpeedSlider();
                EditorGUILayout.PropertyField(timeScaleIndependentSerializedProperty);
                EditorGUILayout.PropertyField(loopsSerializedProperty);
                if (loopsSerializedProperty.intValue != 0)
                    EditorGUILayout.PropertyField(loopTypeSerializedProperty);
                EditorGUILayout.PropertyField(autoKillSerializedProperty);

                if (changedCheck.changed)
                {
                    loopsSerializedProperty.intValue = Mathf.Clamp(loopsSerializedProperty.intValue, -1, int.MaxValue);
                    serializedObject.ApplyModifiedProperties();
                }
            }

            GUI.enabled = wasEnabled;
        }

        private void DrawPlaybackSpeedSlider()
        {
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();

            GUIContent playbackSpeedLabel = new GUIContent("Playback Speed", "Speed of the animation playback.");
            playbackSpeedProperty.floatValue = EditorGUILayout.Slider(playbackSpeedLabel, playbackSpeedProperty.floatValue, 0, 2);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                UpdateSequenceTimeScale();
            }

            GUILayout.FlexibleSpace();
        }

        private void UpdateSequenceTimeScale()
        {
            if (sequencerController.PlayingSequence == null)
                return;

            sequencerController.PlayingSequence.timeScale = sequencerController.PlaybackSpeed * tweenTimeScale;
        }
        #endregion

        #region Callbacks panel
        protected virtual void DrawCallbacks()
        {
            bool wasGUIEnabled = GUI.enabled;
            if (DOTweenEditorPreview.isPreviewing)
                GUI.enabled = false;

            SerializedProperty onStartEventSerializedProperty = serializedObject.FindProperty("onStartEvent");
            SerializedProperty onProgressEventSerializedProperty = serializedObject.FindProperty("onProgressEvent");
            SerializedProperty onFinishedEventSerializedProperty = serializedObject.FindProperty("onFinishedEvent");

            using (EditorGUI.ChangeCheckScope changedCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(onStartEventSerializedProperty);
                EditorGUILayout.PropertyField(onProgressEventSerializedProperty);
                EditorGUILayout.PropertyField(onFinishedEventSerializedProperty);

                if (changedCheck.changed)
                    serializedObject.ApplyModifiedProperties();
            }

            GUI.enabled = wasGUIEnabled;
        }
        #endregion

        #region Steps panel
        private void DrawAnimationSteps()
        {
            bool wasGUIEnabled = GUI.enabled;
            if (DOTweenEditorPreview.isPreviewing)
                GUI.enabled = false;

            reorderableList.DoLayoutList();

            GUI.enabled = wasGUIEnabled;
        }

        private void OnDrawAnimationStepBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index == -1)
                return;

            if (Event.current.type != EventType.Repaint)
                return;

            //Title rect.
            Rect titleRect = new Rect(rect) { height = EditorGUIUtility.singleLineHeight };
            if (isActive)
            {
                ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, true, isFocused, false);
            }
            else
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), new Color(0.1f, 0.1f, 0.1f));
                GUI.skin.box.Draw(titleRect, false, false, false, false);
            }

            //Step progress preview.
            if (AnimationSequencerSettings.GetInstance().VisualizeStepsProgressWhenPreviewing && DOTweenEditorPreview.isPreviewing)
            {
                rect.y += 1;
                StepAnimationData animationData = stepsAnimationData[index];
                Color barColor = new Color(0.4f, 0.38f, 0.1f, 1f);
                Color progressColor = new Color(0.1f, 0.4f, 0.1f, 1f);
                Rect barRect = new Rect(rect) { height = EditorGUIUtility.singleLineHeight };

                if (animationData == null)
                {
                    barColor = new Color(0.4f, 0.1f, 0.1f, 1f);
                    EditorGUI.DrawRect(barRect, barColor);
                }
                else
                {
                    float startTime = animationData.startTime / mainSequenceDuration;
                    float endTime = animationData.endTime / mainSequenceDuration;
                    float extraWidth = (endTime - startTime < 0.01f) ? 1 : 0;

                    barRect.xMin = Mathf.Lerp(rect.xMin, rect.xMax, startTime) - extraWidth;
                    barRect.xMax = Mathf.Lerp(rect.xMin, rect.xMax, endTime) + extraWidth;

                    bool isForward = sequencerController.PlayTypeDirection == AnimationSequencerController.PlayType.Forward;
                    int loops = sequencerController.Loops;
                    float progress = GetCurrentSequencerProgress();
                    if (progress < 1 && loops > 1) progress = progress * loops % 1;
                    bool showProgress = isForward ? progress >= startTime : progress <= endTime;
                    Rect progressRect = new Rect(rect);
                    if (showProgress)
                    {
                        float interpolation_xMin = isForward ? startTime : Mathf.Clamp(progress, startTime, endTime);
                        float interpolation_xMax = isForward ? Mathf.Clamp(progress, startTime, endTime) : endTime;

                        progressRect.xMin = Mathf.Lerp(rect.xMin, rect.xMax, interpolation_xMin) - extraWidth;
                        progressRect.xMax = Mathf.Lerp(rect.xMin, rect.xMax, interpolation_xMax) + extraWidth;
                        progressRect.height = EditorGUIUtility.singleLineHeight;
                    }

                    EditorGUI.DrawRect(barRect, barColor);
                    if (showProgress) EditorGUI.DrawRect(progressRect, progressColor);
                }
            }
        }

        private void OnDrawAnimationStep(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty flowTypeSerializedProperty = element.FindPropertyRelative("flowType");

            FlowType flowType = (FlowType)flowTypeSerializedProperty.enumValueIndex;

            int baseIdentLevel = EditorGUI.indentLevel;

            GUIContent guiContent = new GUIContent(element.displayName);
            AnimationStepBase animationStepBase = null;
            try { animationStepBase = sequencerController.AnimationSteps[index]; } catch (Exception) { }
            if (animationStepBase != null)
            {
                string animationInfo = "";
                if (AnimationSequencerSettings.GetInstance().VisualizeStepsProgressWhenPreviewing && DOTweenEditorPreview.isPreviewing)
                {
                    StepAnimationData stepAnimation = stepsAnimationData[index];
                    animationInfo = stepAnimation == null ? "Unused Step" : stepAnimation.info;
                }

                guiContent = new GUIContent(animationStepBase.GetDisplayNameForEditor(index + 1), animationInfo);
            }

            if (flowType == FlowType.Join)
                EditorGUI.indentLevel = baseIdentLevel + 1;

            rect.height = EditorGUIUtility.singleLineHeight;
            rect.x += 10;
            rect.width -= 10;

            EditorGUI.LabelField(rect, guiContent);
            EditorGUI.PropertyField(rect, element, new GUIContent(""), false);

            EditorGUI.indentLevel = baseIdentLevel;
            // DrawContextInputOnItem(element, index, rect);

            // Verify if the last expanded step changed.
            if (AnimationSequencerSettings.GetInstance().OnlyOneStepExpandedWhileEditing && showStepsPanel && !DOTweenEditorPreview.isPreviewing)
            {
                if (element.isExpanded && index != lastExpandedStepIndex)
                    lastExpandedStepChanged = true;
            }
        }

        private float GetAnimationStepHeight(int index)
        {
            if (index > reorderableList.serializedProperty.arraySize - 1)
                return EditorGUIUtility.singleLineHeight;

            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            return element.GetPropertyDrawerHeight();
        }

        private void OnClickToAddNewAnimationStep(Rect buttonRect, ReorderableList list)
        {
            AnimationStepAdvancedDropdown.Show(buttonRect, OnNewAnimationStepTypeSelected);
        }

        private void OnNewAnimationStepTypeSelected(AnimationStepAdvancedDropdownItem animationStepAdvancedDropdownItem)
        {
            AddNewAnimationStepOfType(animationStepAdvancedDropdownItem.AnimationStepType);
        }

        private void AddNewAnimationStepOfType(Type targetAnimationType)
        {
            SerializedProperty animationStepsProperty = reorderableList.serializedProperty;
            int targetIndex = animationStepsProperty.arraySize;
            animationStepsProperty.InsertArrayElementAtIndex(targetIndex);
            SerializedProperty arrayElementAtIndex = animationStepsProperty.GetArrayElementAtIndex(targetIndex);
            object managedReferenceValue = Activator.CreateInstance(targetAnimationType);
            arrayElementAtIndex.managedReferenceValue = managedReferenceValue;
            arrayElementAtIndex.isExpanded = true;

            //TODO copy from last step would be better here.
            SerializedProperty targetSerializedProperty = arrayElementAtIndex.FindPropertyRelative("target");
            if (targetSerializedProperty != null)
                targetSerializedProperty.objectReferenceValue = (serializedObject.targetObject as AnimationSequencerController)?.gameObject;

            serializedObject.ApplyModifiedProperties();
        }

        private void OnClickToRemoveAnimationStep(ReorderableList list)
        {
            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(list.index);
            reorderableList.serializedProperty.DeleteArrayElementAtIndex(list.index);
            reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
            SerializedPropertyExtensions.ClearPropertyCache(list.serializedProperty.propertyPath);
        }

        private void OnAnimationStepListOrderChanged(ReorderableList list, int oldIndex, int newIndex)
        {
            bool isCyclicRotationRight = true;
            int greatestIndex = oldIndex;
            int smallestIndex = newIndex;
            if (newIndex > oldIndex)
            {
                isCyclicRotationRight = false;
                greatestIndex = newIndex;
                smallestIndex = oldIndex;
            }

            int startIndex = isCyclicRotationRight ? greatestIndex : smallestIndex;
            int count = greatestIndex - smallestIndex + 1;
            float firstHeight = reorderableList.serializedProperty.GetArrayElementAtIndex(startIndex).GetPropertyDrawerHeight();
            bool isFirstExpanded = firstHeight > 18;
            int currentIndex = startIndex;
            int actionsIndex;

            for (int i = 0; i < count; i++)
            {
                SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(currentIndex);

                if (i == count - 1)
                {
                    element.SetPropertyDrawerHeight(firstHeight);
                    element.isExpanded = isFirstExpanded;
                    actionsIndex = startIndex;
                }
                else
                {
                    int nextIndex = isCyclicRotationRight ? currentIndex - 1 : currentIndex + 1;
                    float nextHeight = reorderableList.serializedProperty.GetArrayElementAtIndex(nextIndex).GetPropertyDrawerHeight();
                    element.SetPropertyDrawerHeight(nextHeight);
                    element.isExpanded = nextHeight > 18;
                    actionsIndex = nextIndex;
                }

                if (TryGetActionsExpandedStates(actionsIndex, out List<bool> actionsExpandedStates))
                {
                    SerializedProperty actionsSerializedProperty = element.FindPropertyRelative("actions");
                    bool isTweenStepExpanded = actionsExpandedStates[0];
                    actionsSerializedProperty.isExpanded = isTweenStepExpanded;

                    if (isTweenStepExpanded)
                    {
                        for (int actionIndex = 0; actionIndex < actionsSerializedProperty.arraySize; actionIndex++)
                        {
                            actionsSerializedProperty.GetArrayElementAtIndex(actionIndex).isExpanded = actionsExpandedStates[actionIndex + 1];
                        }
                    }
                }

                if (isCyclicRotationRight)
                    currentIndex--;
                else
                    currentIndex++;
            }

            actionsValuesTaken = false;

            SerializedPropertyExtensions.ClearPropertyCache(list.serializedProperty.propertyPath);
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private bool TryGetActionsExpandedStates(int index, out List<bool> actionsExpandedStates)
        {
            if (expandedActionStates.ContainsKey(index))
            {
                actionsExpandedStates = expandedActionStates[index];
                return true;
            }

            actionsExpandedStates = null;
            return false;
        }

        private void OnMouseDragAnimationStep(ReorderableList list)
        {
            SaveActionsExpandedStates();
        }

        private void SaveActionsExpandedStates()
        {
            if (actionsValuesTaken)
                return;

            expandedActionStates.Clear();
            int reorderableListCount = reorderableList.serializedProperty.arraySize;
            for (int i = 0; i < reorderableListCount; i++)
            {
                bool isTweenStep = sequencerController.AnimationSteps[i].GetType() == typeof(TweenAnimationStep);
                if (isTweenStep)
                {
                    List<bool> actionsExpandedStates = new List<bool>();
                    SerializedProperty actionsSerializedProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("actions");
                    bool isTweenStepExpanded = actionsSerializedProperty.isExpanded;
                    actionsExpandedStates.Add(isTweenStepExpanded);

                    if (isTweenStepExpanded)
                    {
                        int actionsCount = actionsSerializedProperty.arraySize;
                        for (int actionIndex = 0; actionIndex < actionsCount; actionIndex++)
                        {
                            actionsExpandedStates.Add(actionsSerializedProperty.GetArrayElementAtIndex(actionIndex).isExpanded);
                        }
                    }

                    expandedActionStates.Add(i, actionsExpandedStates);
                }
            }

            actionsValuesTaken = true;
        }

        private void OnMouseUpAnimationStep(ReorderableList list)
        {
            actionsValuesTaken = false;
        }

        private void DrawContextInputOnItem(SerializedProperty element, int index, Rect rect1)
        {
            rect1.x -= 24;
            rect1.width += 24;
            Event current = Event.current;

            if (rect1.Contains(current.mousePosition) && current.type == EventType.ContextClick)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy Values"), false, () => ContextClickUtils.SetSource(element));
                if (ContextClickUtils.CanPasteToTarget(element))
                    menu.AddItem(new GUIContent("Paste Values"), false, () => ContextClickUtils.ApplySourceToTarget(element));
                else
                    menu.AddDisabledItem(new GUIContent("Paste Values"));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Duplicate Item"), false, () => DuplicateItem(index));
                menu.AddItem(new GUIContent("Delete Item"), false, () => RemoveItemAtIndex(index));
                menu.ShowAsContext();
                current.Use();
            }
        }

        private void RemoveItemAtIndex(int index)
        {
            reorderableList.serializedProperty.DeleteArrayElementAtIndex(index);
            reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void DuplicateItem(int index)
        {
            SerializedProperty sourceSerializedProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            reorderableList.serializedProperty.InsertArrayElementAtIndex(index + 1);
            SerializedProperty source = reorderableList.serializedProperty.GetArrayElementAtIndex(index + 1);
            ContextClickUtils.CopyPropertyValue(sourceSerializedProperty, source);
            source.serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Generic foldout
        private void DrawFoldoutArea(string title, ref bool foldout, Action additionalInspectorGUI, Action<Rect> additionalHeaderGUI = null)
        {
            using (new EditorGUILayout.VerticalScope("FrameBox"))
            {
                Rect rect = EditorGUILayout.GetControlRect();
                rect.x += 10;
                rect.width -= 10;
                rect.y -= 4;

                foldout = EditorGUI.Foldout(rect, foldout, title);

                additionalHeaderGUI?.Invoke(rect);

                if (foldout)
                    additionalInspectorGUI.Invoke();
            }
        }
        #endregion

        #region Steps animation data
        /// <summary>
        /// Calculate the main sequence duration and "StartTime" of each step relative to the main sequence.
        /// </summary>
        private void CalculateStepsAnimationData()
        {
            //Calculate the main sequence duration and "StartTime" of each step.
            mainSequenceDuration = 0;
            float[] startTimeSteps = new float[sequencerController.AnimationSteps.Length];
            float longestStepDuration = 0;

            for (int i = 0; i < sequencerController.AnimationSteps.Length; i++)
            {
                AnimationStepBase step = sequencerController.AnimationSteps[i];
                float stepDuration = step.GetDuration();
                if (stepDuration == -1)
                {
                    startTimeSteps[i] = stepDuration;
                    continue;
                }

                startTimeSteps[i] = mainSequenceDuration;

                if (i == 0 || step.FlowType == FlowType.Append || stepDuration > longestStepDuration)
                    longestStepDuration = step.GetDuration();

                int nextStepIndex = i + 1;
                if (nextStepIndex >= sequencerController.AnimationSteps.Length || sequencerController.AnimationSteps[nextStepIndex].FlowType == FlowType.Append)
                    mainSequenceDuration += longestStepDuration;
            }

            stepsAnimationData = new StepAnimationData[sequencerController.AnimationSteps.Length];

            //Assign the main sequence duration and "StartTime" to each step. 
            for (int i = 0; i < sequencerController.AnimationSteps.Length; i++)
            {
                float startTime = startTimeSteps[i];
                if (startTime == -1)
                    continue;

                float tweenDuration = sequencerController.AnimationSteps[i].GetDuration();
                stepsAnimationData[i] = new StepAnimationData(tweenDuration / mainSequenceDuration * 100, startTime, startTime + tweenDuration);
            }
        }
        #endregion

        #region Collapse steps (Previewing)
        private void CollapseSteps()
        {
            // Load ReorderableList "ClearCache" Method.
            FindReorderableListClearCacheMethod("Collapse Steps");
            if (cantFindReorderableListClearCacheMethod)
                return;

            // Collapse expanded steps.
            collapsedStepValues.Clear();
            for (int i = 0; i < reorderableList.count; i++)
            {
                SerializedProperty stepProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(i);
                if (!stepProperty.isExpanded)
                    continue;

                collapsedStepValues.Add(i, stepProperty.GetPropertyDrawerHeight());
                stepProperty.isExpanded = false;
                stepProperty.SetPropertyDrawerHeight(EditorGUIUtility.singleLineHeight);
            }

            // Repaint ReorderableList.
            CallReorderableListClearCacheMethod();
        }

        private void ExpandCollapsedSteps()
        {
            if (collapsedStepValues.Count == 0)
                return;

            if (cantFindReorderableListClearCacheMethod)
                return;

            // Expand collapsed steps.
            foreach (var collapsedStepHeight in collapsedStepValues)
            {
                SerializedProperty stepProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(collapsedStepHeight.Key);
                stepProperty.isExpanded = true;
                stepProperty.SetPropertyDrawerHeight(collapsedStepHeight.Value);
            }

            // Repaint ReorderableList.
            CallReorderableListClearCacheMethod();
        }
        #endregion

        #region Only one step expanded
        private void CheckOnlyOneStepExpanded()
        {
            // Load ReorderableList "ClearCache" Method.
            FindReorderableListClearCacheMethod("One Step Expanded");
            if (cantFindReorderableListClearCacheMethod)
                return;

            if (!lastExpandedStepChanged)
                return;

            // Verify only one step is expanded.
            int reorderableListCount = reorderableList.count;
            for (int i = 0; i < reorderableListCount; i++)
            {
                SerializedProperty stepProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(i);

                if (stepProperty.isExpanded)
                {
                    if (lastExpandedStepIndex == i)
                        continue;

                    // Collapse the last expanded step.
                    if (lastExpandedStepIndex != -1 && lastExpandedStepIndex < reorderableListCount)
                    {
                        SerializedProperty lastExpandedStepProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(lastExpandedStepIndex);
                        lastExpandedStepProperty.isExpanded = false;
                        lastExpandedStepProperty.SetPropertyDrawerHeight(18);
                    }

                    // Update "lastExpandedStepIndex" value.
                    lastExpandedStepIndex = i;
                    lastExpandedStepChanged = false;

                    // Repaint ReorderableList.
                    CallReorderableListClearCacheMethod();
                    return;
                }
                else
                {
                    // Reset "lastExpandedStepIndex" if the last expanded step is collapsed.
                    if (lastExpandedStepIndex == i)
                        lastExpandedStepIndex = -1;
                }
            }
        }
        #endregion

        #region ReorderableList "ClearCache" Method
        private void FindReorderableListClearCacheMethod(string featureCallFrom)
        {
            if (cantFindReorderableListClearCacheMethod)
                return;

            if (reorderableListClearCacheMethod != null)
                return;

            var listType = typeof(ReorderableList);
            reorderableListClearCacheMethod = listType.GetMethod("InvalidateCache", BindingFlags.NonPublic | BindingFlags.Instance);
            if (reorderableListClearCacheMethod == null)
            {
                reorderableListClearCacheMethod = listType.GetMethod("ClearCache", BindingFlags.NonPublic | BindingFlags.Instance);
                if (reorderableListClearCacheMethod == null)
                {
                    Debug.LogWarning($"The <b>\"{featureCallFrom}\"</b> feature is not available in this editor version. " +
                        "To avoid seeing this warning, please deselect the option in Preferences > Animation Sequencer.");
                    cantFindReorderableListClearCacheMethod = true;
                }
            }
        }

        private void CallReorderableListClearCacheMethod()
        {
            if (reorderableListClearCacheMethod == null)
                return;

            reorderableListClearCacheMethod.Invoke(reorderableList, null);
        }
        #endregion

        #region Other callbacks
        private void OnEditorPlayModeChanged(PlayModeStateChange playModeState)
        {
            if (playModeState == PlayModeStateChange.ExitingEditMode)
                StopSequence();
        }

        private void PrefabSaving(GameObject gameObject)
        {
            StopSequence();
        }
        #endregion
    }

    #region Step animation data
    /// <summary>
    /// Class used to show visual animation data for each step.
    /// </summary>
    public class StepAnimationData
    {
        /// <summary>
        /// Percentage duration of this step relative to the main sequence.
        /// </summary>
        public float percentageDuration;
        /// <summary>
        /// The time this step starts relative to the main sequence.
        /// </summary>
        public float startTime;
        /// <summary>
        /// The time this step ends relative to the main sequence.
        /// </summary>
        public float endTime;
        /// <summary>
        /// Data summary.
        /// </summary>
        public string info;

        public StepAnimationData(float percentageDuration, float startTime, float endTime)
        {
            this.percentageDuration = percentageDuration;
            this.startTime = startTime;
            this.endTime = endTime;

            float duration = endTime - startTime;
            info = $"Duration: {NumberFormatter.FormatDecimalPlaces(duration)}s ({NumberFormatter.FormatDecimalPlaces(percentageDuration)}%)\n" +
                $"Start time: {NumberFormatter.FormatDecimalPlaces(startTime)}s\n" +
                $"End time: {NumberFormatter.FormatDecimalPlaces(endTime)}s";
        }
    }
    #endregion
}
#endif