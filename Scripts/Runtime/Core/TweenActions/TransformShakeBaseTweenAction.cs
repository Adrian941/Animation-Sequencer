#if DOTWEEN_ENABLED
using System;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Created by Pablo Huaxteco
    [Serializable]
    public abstract class TransformShakeBaseTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);
        public override string[] ExcludedFields => new string[] { "direction", "ease", "relative" };

        [SerializeField]
        protected Vector3 strength = Vector3.one;
        public Vector3 Strength
        {
            get => strength;
            set => strength = value;
        }

        [SerializeField]
        protected int vibrato = 10;
        public int Vibrato
        {
            get => vibrato;
            set => vibrato = value;
        }

        [SerializeField]
        protected float randomness = 90;
        public float Randomness
        {
            get => randomness;
            set => randomness = value;
        }

        [SerializeField]
        protected bool fadeout = true;
        public bool Fadeout
        {
            get => fadeout;
            set => fadeout = value;
        }
    }
}
#endif