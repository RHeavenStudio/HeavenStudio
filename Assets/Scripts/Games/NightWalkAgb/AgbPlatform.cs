using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AgbNightWalk
{
    public class AgbPlatform : MonoBehaviour
    {
        private double startBeat;
        private double endBeat;
        private AgbPlatformHandler handler;
        public void Init(double beat, double hitBeat, AgbPlatformHandler handlerToPut)
        {
            handler = handlerToPut;
            startBeat = beat;
            endBeat = hitBeat;
            if (startBeat < endBeat)
            {
                AgbNightWalk.instance.ScheduleInput(startBeat, endBeat - startBeat, InputType.STANDARD_DOWN, Just, Miss, Empty);
            }
        }

        private void Awake()
        {
            Update();
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                float normalizedBeat = cond.GetPositionFromBeat(startBeat, endBeat - startBeat);

                float newPosX = Mathf.LerpUnclamped(handler.playerXPos + (float)((endBeat - startBeat) * handler.platformDistance), handler.playerXPos, normalizedBeat);

                transform.localPosition = new Vector3(newPosX, handler.defaultYPos);

                if (cond.songPositionInBeats > endBeat + (handler.platformCount * 0.5f))
                {
                    ResetInput();
                }
            }
        }

        private void ResetInput()
        {
            double newStartBeat = endBeat + (handler.platformCount * 0.5f);
            Init(newStartBeat, newStartBeat + (handler.platformCount * 0.5f), handler);
        }
        private void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1 || state <= -1)
            {
                return;
            }
        }

        private void Miss(PlayerActionEvent caller)
        {

        }
        
        private void Empty(PlayerActionEvent caller) { }
    }
}

