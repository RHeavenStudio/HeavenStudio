using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using UnityEngine.UIElements;

namespace HeavenStudio.Games.Scripts_RhythmTweezers
{
    public class LongHair : MonoBehaviour
    {
        public float createBeat;
        public GameObject hairSprite;
        public GameObject stubbleSprite;
        private RhythmTweezers game;
        private Tweezers tweezers;
        private Animator anim;
        private int pluckState = 0;

        public GameObject holder;
        public GameObject loop;

        private Sound pullSound;

        private float inputBeat;

        PlayerActionEvent endEvent;

        private void Awake()
        {
            game = RhythmTweezers.instance;
            anim = GetComponent<Animator>();
            tweezers = game.Tweezers;
        }

        public void StartInput(float beat, float length)
        {
            inputBeat = beat + length;
            game.ScheduleInput(beat, length, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, StartJust, StartMiss, Out);
        }

        private void Update()
        {
            if (pluckState == 1)
            {
                Vector3 tst = tweezers.tweezerSpriteTrans.position;
                var hairDirection = new Vector3(tst.x + 0.173f, tst.y) - holder.transform.position;
                holder.transform.rotation = Quaternion.FromToRotation(Vector3.down, hairDirection);

                float normalizedBeat = Conductor.instance.GetPositionFromBeat(inputBeat, 0.5f);
                anim.Play("LoopPull", 0, normalizedBeat);
                tweezers.anim.Play("Tweezers_LongPluck", 0, normalizedBeat);
                if (!game.IsExpectingInputNow(InputType.STANDARD_UP | InputType.DIRECTION_DOWN_UP) && PlayerInput.PressedUp(true) && normalizedBeat < 1f)
                {
                    EndEarly();
                    endEvent.Disable();
                }
                // Auto-release if holding at release time.
                if (normalizedBeat >= 1f)
                    endEvent.Hit(0, 1);
            }

            loop.transform.localScale = Vector2.one / holder.transform.localScale;
        }

        public void EndAce()
        {
            if (pluckState != 1) return;
            tweezers.LongPluck(true, this);
            tweezers.hitOnFrame++;

            if (pullSound != null)
                pullSound.Stop();

            pluckState = -1;
        }

        public void EndEarly()
        {
            var normalized = Conductor.instance.GetPositionFromBeat(inputBeat, 0.5f);
            anim.Play("LoopPullReverse", 0, normalized);
            tweezers.anim.Play("Tweezers_Idle", 0, 0);

            if (pullSound != null)
                pullSound.Stop();

            pluckState = -1;
        }

        private void StartJust(PlayerActionEvent caller, float state)
        {
            // don't count near misses
            if (state >= 1f || state <= -1f) {
                pluckState = -1;
                return; 
            }
            pullSound = Jukebox.PlayOneShotGame($"rhythmTweezers/longPull{UnityEngine.Random.Range(1, 5)}");
            pluckState = 1;
            endEvent = game.ScheduleInput(inputBeat, 0.5f, InputType.STANDARD_UP | InputType.DIRECTION_DOWN_UP, EndJust, Out, Out);
        }

        private void StartMiss(PlayerActionEvent caller) 
        {
            // this is where perfect challenge breaks
        }

        private void Out(PlayerActionEvent caller) {}

        private void EndJust(PlayerActionEvent caller, float state)
        {
            if (state <= -1f) {
                EndEarly();
                return; 
            }
            EndAce();
        }
    }
}