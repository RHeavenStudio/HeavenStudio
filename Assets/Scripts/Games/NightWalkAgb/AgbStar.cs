using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_AgbNightWalk
{
    public class AgbStar : MonoBehaviour
    {
        private float originX;
        private float originY;
        private AgbStarHandler handler;

        public void Init(float x, float y, AgbStarHandler handlerToPut)
        {
            originX = x;
            originY = y;
            handler = handlerToPut;
            Update();
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                transform.localPosition = handler.GetRelativePosition(ref originX, ref originY);
            }
        }
    }
}

