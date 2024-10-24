#if DOTWEEN_ENABLED
using System;
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
        private bool showPreviewPanel = true;
        private bool showSettingsPanel;
        private bool showCallbacksPanel;
        private bool showStepsPanel;
        private float tweenTimeScale = 1f;
        private bool wasShowingStepsPanel;
        private bool justStartPreviewing;
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
        }

        private void InitializeReorderableList()
        {
            reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("animationSteps"), true, false, true, true);
            reorderableList.drawElementCallback += OnDrawAnimationStep;
            reorderableList.elementHeightCallback += GetAnimationStepHeight;
            reorderableList.onAddDropdownCallback += OnClickToAddNew;
            reorderableList.onRemoveCallback += OnClickToRemove;
            reorderableList.onReorderCallbackWithDetails += OnListOrderChanged;
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
            reorderableList.drawElementCallback -= OnDrawAnimationStep;
            reorderableList.elementHeightCallback -= GetAnimationStepHeight;
            reorderableList.onAddDropdownCallback -= OnClickToAddNew;
            reorderableList.onRemoveCallback -= OnClickToRemove;
            reorderableList.onReorderCallbackWithDetails -= OnListOrderChanged;
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
            if (sequencerController.IsResetRequired())
                SetDefaults();

            DrawFoldoutArea("Preview", ref showPreviewPanel, DrawPreviewControls);
            DrawFoldoutArea("Settings", ref showSettingsPanel, DrawSettings);
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
        }
        #endregion

        #region Sequencer defaults
        private void SetDefaults()
        {
            sequencerController = target as AnimationSequencerController;

            if (sequencerController != null)
            {
                sequencerController.SetAutoplayMode(AnimationControllerDefaults.Instance.AutoplayMode);
                sequencerController.SetPlayOnStart(AnimationControllerDefaults.Instance.PlayOnStart);
                sequencerController.SetPauseOnStart(AnimationControllerDefaults.Instance.PauseOnStart);
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
        private void DrawPreviewControls()
        {
            DrawMediaPlayerControlButtons();
            DrawTimeScaleSlider();
            DrawProgressSlider();
            DrawDurationInfo();
        }

        private void DrawMediaPlayerControlButtons()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool guiEnabled = GUI.enabled;

            GUIStyle previewButtonStyle = new GUIStyle(GUI.skin.button);
            previewButtonStyle.fixedWidth = previewButtonStyle.fixedHeight = 40;

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

                    if (AnimationSequencerSettings.GetInstance().AutoHideStepsWhenPreviewing)
                        showStepsPanel = wasShowingStepsPanel;
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

            if (AnimationSequencerSettings.GetInstance().AutoHideStepsWhenPreviewing)
                showStepsPanel = false;
        }

        private void StopSequence()
        {
            if (DOTweenEditorPreview.isPreviewing)
            {
                sequencerController.ResetToInitialState();
                sequencerController.ClearPlayingSequence();
                DOTweenEditorPreview.Stop();
            }
        }

        private void DrawTimeScaleSlider()
        {
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("TimeScale");
            tweenTimeScale = EditorGUILayout.Slider(tweenTimeScale, 0, 2);

            UpdateSequenceTimeScale();

            GUILayout.FlexibleSpace();
        }

        private void DrawProgressSlider()
        {
            GUILayout.FlexibleSpace();

            EditorGUI.BeginChangeCheck();
            float tweenProgress = GetCurrentSequencerProgress();

            EditorGUILayout.LabelField("Progress");
            tweenProgress = EditorGUILayout.Slider(tweenProgress, 0, 1);

            if (EditorGUI.EndChangeCheck())
            {
                SetProgress(tweenProgress);
            }

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
        private void DrawSettings()
        {
            bool wasEnabled = GUI.enabled;
            if (DOTweenEditorPreview.isPreviewing)
                GUI.enabled = false;

            SerializedProperty autoPlayModeSerializedProperty = serializedObject.FindProperty("autoplayMode");
            SerializedProperty pauseOnStartSerializedProperty = serializedObject.FindProperty("startPaused");
            SerializedProperty timeScaleIndependentSerializedProperty = serializedObject.FindProperty("timeScaleIndependent");
            SerializedProperty sequenceDirectionSerializedProperty = serializedObject.FindProperty("playType");
            SerializedProperty updateTypeSerializedProperty = serializedObject.FindProperty("updateType");
            SerializedProperty autoKillSerializedProperty = serializedObject.FindProperty("autoKill");
            SerializedProperty loopsSerializedProperty = serializedObject.FindProperty("loops");
            SerializedProperty loopTypeSerializedProperty = serializedObject.FindProperty("loopType");

            using (EditorGUI.ChangeCheckScope changedCheck = new EditorGUI.ChangeCheckScope())
            {
                AnimationSequencerController.AutoplayType autoplayMode = (AnimationSequencerController.AutoplayType)autoPlayModeSerializedProperty.enumValueIndex;
                EditorGUILayout.PropertyField(autoPlayModeSerializedProperty);
                if (autoplayMode != AnimationSequencerController.AutoplayType.Nothing)
                    EditorGUILayout.PropertyField(pauseOnStartSerializedProperty);
                DrawPlaybackSpeedSlider();
                EditorGUILayout.PropertyField(timeScaleIndependentSerializedProperty);
                EditorGUILayout.PropertyField(sequenceDirectionSerializedProperty);
                EditorGUILayout.PropertyField(updateTypeSerializedProperty);
                EditorGUILayout.PropertyField(autoKillSerializedProperty);
                EditorGUILayout.PropertyField(loopsSerializedProperty);

                if (loopsSerializedProperty.intValue != 0)
                    EditorGUILayout.PropertyField(loopTypeSerializedProperty);

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

            var playbackSpeedProperty = serializedObject.FindProperty("playbackSpeed");
            playbackSpeedProperty.floatValue = EditorGUILayout.Slider("Playback Speed", playbackSpeedProperty.floatValue, 0, 2);

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
            SerializedProperty onFinishedEventSerializedProperty = serializedObject.FindProperty("onFinishedEvent");
            SerializedProperty onProgressEventSerializedProperty = serializedObject.FindProperty("onProgressEvent");

            using (EditorGUI.ChangeCheckScope changedCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(onStartEventSerializedProperty);
                EditorGUILayout.PropertyField(onFinishedEventSerializedProperty);
                EditorGUILayout.PropertyField(onProgressEventSerializedProperty);

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
                guiContent = new GUIContent(animationStepBase.GetDisplayNameForEditor(index + 1));

            if (flowType == FlowType.Join)
                EditorGUI.indentLevel = baseIdentLevel + 1;

            rect.height = EditorGUIUtility.singleLineHeight;
            rect.x += 10;
            rect.width -= 10;

            EditorGUI.LabelField(rect, guiContent);
            EditorGUI.PropertyField(rect, element, new GUIContent(""), false);

            EditorGUI.indentLevel = baseIdentLevel;
            // DrawContextInputOnItem(element, index, rect);
        }

        private float GetAnimationStepHeight(int index)
        {
            if (index > reorderableList.serializedProperty.arraySize - 1)
                return EditorGUIUtility.singleLineHeight;

            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            return element.GetPropertyDrawerHeight();
        }

        private void OnClickToAddNew(Rect buttonRect, ReorderableList list)
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

            //TODO copy from last step would be better here.
            SerializedProperty targetSerializedProperty = arrayElementAtIndex.FindPropertyRelative("target");
            if (targetSerializedProperty != null)
                targetSerializedProperty.objectReferenceValue = (serializedObject.targetObject as AnimationSequencerController)?.gameObject;

            serializedObject.ApplyModifiedProperties();
        }

        private void OnClickToRemove(ReorderableList list)
        {
            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(list.index);
            reorderableList.serializedProperty.DeleteArrayElementAtIndex(list.index);
            reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
            SerializedPropertyExtensions.ClearPropertyCache(list.serializedProperty.propertyPath);
        }

        private void OnListOrderChanged(ReorderableList list, int oldIndex, int newIndex)
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
            for (int i = 0; i < count; i++)
            {
                SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(startIndex);

                if (i == count - 1)
                {
                    element.SetPropertyDrawerHeight(firstHeight);
                }
                else
                {
                    int nextIndex = isCyclicRotationRight ? startIndex - 1 : startIndex + 1;
                    float nextHeight = reorderableList.serializedProperty.GetArrayElementAtIndex(nextIndex).GetPropertyDrawerHeight();
                    element.SetPropertyDrawerHeight(nextHeight);
                }

                if (isCyclicRotationRight)
                    startIndex--;
                else
                    startIndex++;
            }

            SerializedPropertyExtensions.ClearPropertyCache(list.serializedProperty.propertyPath);
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
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
        private void DrawFoldoutArea(string title, ref bool foldout, Action additionalInspectorGUI)
        {
            using (new EditorGUILayout.VerticalScope("FrameBox"))
            {
                Rect rect = EditorGUILayout.GetControlRect();
                rect.x += 10;
                rect.width -= 10;
                rect.y -= 4;

                foldout = EditorGUI.Foldout(rect, foldout, title);

                if (foldout)
                    additionalInspectorGUI.Invoke();
            }
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
}
#endif