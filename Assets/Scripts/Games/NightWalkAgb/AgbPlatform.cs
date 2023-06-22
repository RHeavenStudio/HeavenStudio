using HeavenStudio.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AgbNightWalk
{
    public class AgbPlatform : MonoBehaviour
    {
        public enum PlatformType
        {
            Flower = 1,
            Lollipop = 2,
            Umbrella = 3
        }
        private double startBeat;
        private double endBeat;
        [NonSerialized] public AgbPlatformHandler handler;
        private Animator anim;

        private AgbNightWalk game;

        private PlatformType type = PlatformType.Flower;

        private float additionalHeight = 0f;
        private int additionalHeightInUnits = 0;
        private int lastAdditionalHeightInUnits = 0;

        [SerializeField] private GameObject platform;

        public void StartInput(double beat, double hitBeat)
        {
            if (game == null) game = AgbNightWalk.instance;
            lastAdditionalHeightInUnits = game.FindHeightUnitsAtBeat(hitBeat);
            additionalHeightInUnits = game.FindHeightUnitsAtBeat(hitBeat + 1);
            additionalHeight = lastAdditionalHeightInUnits * handler.heightAmount;
            platform.SetActive(lastAdditionalHeightInUnits == additionalHeightInUnits);
            startBeat = beat;
            endBeat = hitBeat;
            if (startBeat < endBeat)
            {
                AgbNightWalk.instance.ScheduleInput(startBeat, endBeat - startBeat, InputType.STANDARD_DOWN, Just, Miss, Empty);
            }
        }

        private void Awake()
        {
            game = AgbNightWalk.instance;
            anim = GetComponent<Animator>();
            Update();
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                float normalizedBeat = cond.GetPositionFromBeat(startBeat, endBeat - startBeat);

                float newPosX = Mathf.LerpUnclamped(handler.playerXPos + (float)((endBeat - startBeat) * handler.platformDistance), handler.playerXPos, normalizedBeat);

                transform.localPosition = new Vector3(newPosX, handler.defaultYPos + additionalHeight);

                if (cond.songPositionInBeats > endBeat + (handler.platformCount * 0.5f))
                {
                    ResetInput();
                }
            }
        }

        private void ResetInput()
        {
            double newStartBeat = endBeat + (handler.platformCount * 0.5f);
            anim.Play("Idle", 0, 0);
            StartInput(newStartBeat, newStartBeat + (handler.platformCount * 0.5f));
        }
        private void Just(PlayerActionEvent caller, float state)
        {
            handler.RaiseHeight(Conductor.instance.songPositionInBeats, lastAdditionalHeightInUnits, additionalHeightInUnits);
            game.playYan.Jump(Conductor.instance.songPositionInBeats);
            if (state >= 1 || state <= -1)
            {
                return;
            }
            SoundByte.PlayOneShotGame("nightWalkAgb/jump" + (int)type);
            anim.DoScaledAnimationAsync("Flower", 0.5f);
        }

        private void Miss(PlayerActionEvent caller)
        {

        }
        
        private void Empty(PlayerActionEvent caller) { }
    }
}

