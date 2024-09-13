#if DOTWEEN_ENABLED
using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    // Created by Pablo Huaxteco
    [Serializable]
    public sealed class TransformPathTweenAction : TweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);
        public override string DisplayName => "Path";
        public override string[] ExcludedFields
        {
            get
            {
                List<string> result = new List<string> { "direction" };

                if (inputType == InputType.Vector)
                {
                    result.Add("targets");
                    if (relative) result.Add("local");
                }
                else
                {
                    result.Add("positions");
                    result.Add("local");
                }

                return result.ToArray();
            }
        }

        [SerializeField]
        private InputType inputType;
        public InputType InputType
        {
            get => inputType;
            set => inputType = value;
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

        [SerializeField]
        private bool closePath;
        public bool ClosePath
        {
            get => closePath;
            set => closePath = value;
        }

        private Transform targetTransform;
        private Vector3 originalPosition;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            targetTransform = target.transform;

            TweenerCore<Vector3, Path, PathOptions> tween;
            if (inputType == InputType.Vector && local)
            {
                originalPosition = targetTransform.localPosition;
                tween = targetTransform.DOLocalPath(GetPositions(), duration, pathType, pathMode, resolution, gizmoColor);
            }
            else
            {
                originalPosition = targetTransform.position;
                tween = targetTransform.DOPath(GetPositions(), duration, pathType, pathMode, resolution, gizmoColor);
            }
            tween.SetOptions(closePath);

            return tween;
        }

        private Vector3[] GetPositions()
        {
            switch (inputType)
            {
                case InputType.Vector:
                    return GetPositionsFromVectorInput();
                case InputType.Object:
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