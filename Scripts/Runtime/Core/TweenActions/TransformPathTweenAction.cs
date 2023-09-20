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
    public sealed class TransformPathTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);
        public override string DisplayName => "Path";

        [SerializeField]
        private TypeInput typeInput;
        public TypeInput TypeInput
        {
            get => typeInput;
            set => typeInput = value;
        }

        [SerializeField]
        private Vector3[] positions;
        public Vector3[] Positions
        {
            get => positions;
            set => positions = value;
        }

        [SerializeField]
        private bool local;
        public bool Local
        {
            get => local;
            set => local = value;
        }

        [SerializeField]
        private Transform[] targets;
        public Transform[] Targets
        {
            get => targets;
            set => targets = value;
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
            if (typeInput == TypeInput.Vector && local)
            {
                originalPosition = targetTransform.localPosition;
                tween = targetTransform.DOLocalPath(GetPositions(), duration, pathType, pathMode, resolution, gizmoColor);
            }
            else
            {
                originalPosition = targetTransform.position;
                tween = targetTransform.DOPath(GetPositions(), duration, pathType, pathMode, resolution, gizmoColor);
            }

            return tween;
        }

        private Vector3[] GetPositions()
        {
            switch (typeInput)
            {
                case TypeInput.Vector:
                    return GetPositionsFromVectorInput();
                case TypeInput.Object:
                    return GetPositionsFromObjectInput();
            }

            return null;
        }

        private Vector3[] GetPositionsFromVectorInput()
        {
            return positions;
        }

        private Vector3[] GetPositionsFromObjectInput()
        {
            Vector3[] result = new Vector3[targets.Length];

            for (int i = 0; i < targets.Length; i++)
            {
                Transform pointTransform = targets[i];
                result[i] = pointTransform.position;
            }

            return result;
        }

        public override void ResetToInitialState()
        {
            if (targetTransform == null)
                return;

            if (local)
                targetTransform.localPosition = originalPosition;
            else
                targetTransform.position = originalPosition;
        }
    }
}
#endif