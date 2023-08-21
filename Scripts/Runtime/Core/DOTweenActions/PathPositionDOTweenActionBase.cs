#if DOTWEEN_ENABLED
using System;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public sealed class PathPositionDOTweenActionBase : PathDOTweenActionBase
    {
        public override string DisplayName => "Move to Path Positions";

        [SerializeField]
        private Vector3[] positions;
        public Vector3[] Positions => positions;

        protected override Vector3[] GetPathPositions()
        {
            return positions;
        }
    }
}
#endif