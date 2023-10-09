using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    public sealed class AnimationSequencerSettings : ScriptableObjectForPreferences<AnimationSequencerSettings>
    {
        [SerializeField]
        private bool autoHideStepsWhenPreviewing = true;
        public bool AutoHideStepsWhenPreviewing => autoHideStepsWhenPreviewing;

        [SerializeField]
        [Tooltip("Show or not the animation data for each step in the inspector like the blue progress bar.")]
        private bool showStepAnimationInfo = true;
        public bool ShowStepAnimationInfo => showStepAnimationInfo;

        [SettingsProvider]
        private static SettingsProvider SettingsProvider()
        {
            return CreateSettingsProvider("Animation Sequencer", null);
        }
    }
}
