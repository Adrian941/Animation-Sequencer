#if DOTWEEN_ENABLED
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public abstract class PathDOTweenActionBase : DOTweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);

        [SerializeField]
        protected bool isLocal;
        public bool IsLocal
        {
            get => isLocal;
            set => isLocal = value;
        }

        [SerializeField]
        private Color gizmoColor;
        public Color GizmoColor
        {
            get => gizmoColor;
            set => gizmoColor = value;
        }

        [SerializeField]
        private int resolution = 10;
        public int Resolution
        {
            get => resolution;
            set => resolution = value;
        }

        [SerializeField]
        private PathMode pathMode = PathMode.Full3D;
        public PathMode PathMode
        {
            get => pathMode;
            set => pathMode = value;
        }

        [SerializeField]
        private PathType pathType = PathType.CatmullRom;
        public PathType PathType
        {
            get => pathType;
            set => pathType = value;
        }

        private Transform targetTransform;
        private Vector3 originalPosition;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            targetTransform = target.transform;

            TweenerCore<Vector3, Path, PathOptions> tween;
            if (!isLocal)
            {
                tween = targetTransform.DOPath(GetPathPositions(), duration, pathType, pathMode, resolution, gizmoColor);
                originalPosition = targetTransform.position;
            }
            else
            {
                tween = targetTransform.DOLocalPath(GetPathPositions(), duration, pathType, pathMode, resolution, gizmoColor);
                originalPosition = targetTransform.localPosition;
            }

            return tween;
        }

        protected abstract Vector3[] GetPathPositions();

        public override void ResetToInitialState()
        {
            if (targetTransform == null)
                return;

            if (isLocal)
                targetTransform.localPosition = originalPosition;
            else
                targetTransform.position = originalPosition;
        }
    }
}
#endif