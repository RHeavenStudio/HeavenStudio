using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio
{
    public class FollowPath : MonoBehaviour
    {
        [SerializeField] Vector3 offset;
        protected Vector3 lastRealPos;

        [Serializable]
        public struct PathPos
        {
            [SerializeField] public Transform pos;
            [SerializeField] public float height;
            [SerializeField] public float rotSpeed;
            [SerializeField] public float duration;
            [SerializeField] public bool destroy;
            [SerializeField] public bool useLastRealPos;
        }

        [Serializable]
        public struct Path
        {
            [SerializeField] public string name;
            [SerializeField] public bool preview;
            [SerializeField] public PathPos[] positions;
        }

        protected virtual void UpdateLastRealPos()
        {
            lastRealPos = transform.position;
        }

        protected virtual Vector3 GetPathPositionFromBeat(Path path, float currentTime, float startTime = 0f)
        {
            if (path.positions.Length < 2)
                return transform.position;
            
            PathPos currentPos = path.positions[0];
            PathPos nextPos = path.positions[1];
            float currentPosTime = 0f;
            for (int i = 0; i < path.positions.Length - 1; i++)
            {
                currentPos = path.positions[i];
                nextPos = path.positions[i + 1];
                if (currentTime - startTime > currentPosTime && currentTime - startTime <= currentPosTime + currentPos.duration)
                {
                    break;
                }
                if (i + 1 < path.positions.Length - 1)
                    currentPosTime += currentPos.duration;
                else
                    break;
            }
            if (currentPos.pos == null || nextPos.pos == null)
                return transform.position;
            
            Vector3 startPos = currentPos.pos.position;
            Vector3 endPos = nextPos.pos.position;
            if (currentPos.useLastRealPos)
                startPos = lastRealPos;
            
            float time = (currentTime - startTime - currentPosTime) / currentPos.duration;
            Vector3 pos = Vector3.LerpUnclamped(startPos, endPos, time);
            float yMul = time * 2f - 1f;
            float yWeight = -(yMul * yMul) + 1f;
            pos.y += yWeight * currentPos.height;
            return pos + offset;
        }
    }
}