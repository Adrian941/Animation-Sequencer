#if DOTWEEN_ENABLED
using System;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Created by Pablo Huaxteco
    [Serializable]
    public abstract class TransformPunchBaseTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);
        public override string[] ExcludedFields => new string[] { "direction", "ease", "relative" };

        [SerializeField]
        protected Vector3 punch = Vector3.one;
        public Vector3 Punch
        {
            get => punch;
            set => punch = value;
        }

        [SerializeField]
        protected int vibrato = 10;
        public int Vibrato
        {
            get => vibrato;
            set => vibrato = value;
        }

        [SerializeField]
        protected float elasticity = 1f;
        public float Elasticity
        {
            get => elasticity;
            set => elasticity = value;
        }
    }
}
#endif