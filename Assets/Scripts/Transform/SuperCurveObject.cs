using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio
{
    public class SuperCurveObject : MonoBehaviour
    {
        [SerializeField] protected Vector3 offset;
        protected Vector3 lastRealPos;
        protected PathPos currentPathPos;

        [Serializable]
        public struct PathPos
        {
            [SerializeField] public string tag;
            [SerializeField] public Vector3 pos;
            [SerializeField] public Transform target;
            [SerializeField] public float height;
            [SerializeField] public float duration;
            [SerializeField] public bool useLastRealPos;
            [SerializeField] public PathValue[] values;
        }

        [Serializable]
        public struct Path
        {
            [SerializeField] public string name;
            [SerializeField] public bool preview;
            [SerializeField] public GameObject anchor;
            [SerializeField] public PathPos[] positions;
        }

        [Serializable]
        public struct PathValue
        {
            [SerializeField] public string key;
            [SerializeField] public float value;
        }

        public static PathPos GetPathPosByTag(Path path, string tag)
        {
            foreach (PathPos pos in path.positions)
            {
                if (pos.tag == tag)
                    return pos;
            }
            return default(PathPos);
        }

        public static PathPos GetPathPosByTime(Path path, double currentTime, out double posTime, out PathPos nextPos, out PathPos prevPos, out bool pastCurve)
        {
            pastCurve = false;
            PathPos currentPos = path.positions[0];
            prevPos = path.positions[0];
            nextPos = path.positions[1];
            float currentPosTime = 0f;
            for (int i = 0; i < path.positions.Length - 1; i++)
            {
                prevPos = currentPos;
                currentPos = path.positions[i];
                nextPos = path.positions[i + 1];
                if (currentTime >= currentPosTime && currentTime < currentPosTime + currentPos.duration)
                {
                    posTime = currentPosTime;
                    return currentPos;
                }
                if (i + 1 < path.positions.Length - 1)
                    currentPosTime += currentPos.duration;
                else
                {
                    pastCurve = true;
                    posTime = currentPosTime;
                    return currentPos;
                }
            }
            nextPos = default(PathPos);
            prevPos = default(PathPos);
            posTime = 0f;
            return default(PathPos);
        }

        public static PathPos GetPathPosByTime(Path path, double currentTime)
        {
            return GetPathPosByTime(path, currentTime, out _, out _, out _, out _);
        }

        public static float GetPointTimeByTag(Path path, string tag)
        {
            float currentTime = 0f;
            foreach (PathPos pos in path.positions)
            {
                if (pos.tag == tag)
                    return currentTime;
                currentTime += pos.duration;
            }
            return 0f;
        }

        protected virtual void UpdateLastRealPos()
        {
            lastRealPos = transform.position;
        }

        protected virtual float GetPathValue(string key)
        {
            if (currentPathPos.values == null)
                return 0f;
            foreach (PathValue value in currentPathPos.values)
            {
                if (value.key == key)
                {
                    return value.value;
                }
            }
            return 0f;
        }

        protected virtual Vector3 GetPathPositionFromBeat(Path path, double currentTime, out double heightOffset, double startTime = 0f)
        {
            heightOffset = 0f;
            Vector3 anchorPos = Vector3.zero;
            if (path.anchor != null)
                anchorPos = path.anchor.transform.position;
            if (path.positions.Length < 2)
                return transform.position;
            
            PathPos nextPos = path.positions[0];
            double currentPosTime = 0;
            PathPos currentPos = GetPathPosByTime(path, currentTime - startTime, out currentPosTime, out nextPos, out  _,  out _);
            currentPathPos = currentPos;
            if (currentPos.pos == null || nextPos.pos == null)
                return transform.position;
            
            Vector3 startPos = currentPos.pos;
            if (currentPos.useLastRealPos)
                startPos = lastRealPos;
            else if (currentPos.target != null)
                startPos = currentPos.target.position;
            Vector3 endPos = nextPos.pos;
            if (nextPos.target != null)
                endPos = nextPos.target.position;
            
            double time = (currentTime - startTime - currentPosTime) / currentPos.duration;
            Vector3 pos = Vector3.LerpUnclamped(startPos, endPos, (float) time);
            double yMul = time * 2f - 1f;
            double yWeight = -(yMul * yMul) + 1f;
            heightOffset = yWeight * currentPos.height;
            pos.y += (float) heightOffset;
            return pos + offset + anchorPos;
        }

        protected virtual Vector3 GetPathPositionFromBeat(Path path, double currentTime, double startTime = 0f)
        {
            return GetPathPositionFromBeat(path, currentTime, out _, startTime);
        }

        // Editor gizmo to draw trajectories
        const float TRAJECTORY_STEP = 0.1f;
        public virtual void DrawEditorGizmo(Path path)
        {
            if (path.positions.Length > 1)
            {
                Vector3 anchorPos = Vector3.zero;
                if (path.anchor != null)
                    anchorPos = path.anchor.transform.position;
                for (int i = 0; i < path.positions.Length - 1; i++)
                {
                    PathPos pos = path.positions[i];
                    PathPos nextPos = path.positions[i + 1];
                    if (pos.pos == null || nextPos.pos == null)
                        return;

                    Vector3 startPos = pos.pos;
                    if (pos.target != null)
                        startPos = pos.target.position;
                    Vector3 endPos = nextPos.pos;
                    if (nextPos.target != null)
                        endPos = nextPos.target.position;

                    // draw a curve between the two points using the path height
                    List<Vector3> points = new List<Vector3>();
                    for (float t = 0; t < 1; t += TRAJECTORY_STEP)
                    {
                        float yMul = t * 2f - 1f;
                        float yWeight = -(yMul * yMul) + 1f;
                        Vector3 p = Vector3.LerpUnclamped(startPos, endPos, t);
                        p.y += yWeight * pos.height;
                        points.Add(p + anchorPos);
                    }

                    points.Add(endPos + anchorPos);

                    for (int j = 0; j < points.Count - 1; j++)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(points[j], points[j + 1]);
                    }

                    Gizmos.DrawSphere(startPos, 0.1f);
                    Gizmos.DrawSphere(endPos, 0.1f);
                }
            }
        }
    }
}