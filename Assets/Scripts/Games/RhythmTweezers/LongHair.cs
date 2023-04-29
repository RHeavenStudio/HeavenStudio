using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

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

        PlayerActionEvent pluckEvent;
        PlayerActionEvent endEvent;
        InputType endInput;

        private void Awake()
        {
            game = RhythmTweezers.instance;
            anim = GetComponent<Animator>();
            tweezers = game.Tweezers;
        }

        private void Start() {
            game.ScheduleInput(createBeat, game.tweezerBeatOffset + game.beatInterval, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, StartJust, StartMiss, Out);
        }

        private void Update()
        {
            if (pluckState == 1)
            {
                bool input = PlayerInput.PressedUp();
                if (endInput == InputType.DIRECTION_UP) input = PlayerInput.GetAnyDirectionUp();
                if (input && !game.IsExpectingInputNow(endInput))
                {
                    endEvent.isEligible = false;
                    EndEarly();
                    return;
                }

                Vector3 tst = tweezers.tweezerSpriteTrans.position;
                var hairDirection = new Vector3(tst.x + 0.173f, tst.y) - holder.transform.position;
                holder.transform.rotation = Quaternion.FromToRotation(Vector3.down, hairDirection);

                float normalizedBeat = Conductor.instance.GetPositionFromBeat(createBeat + game.tweezerBeatOffset + game.beatInterval, 0.5f);
                anim.Play("LoopPull", 0, normalizedBeat);
                tweezers.anim.Play("Tweezers_LongPluck", 0, normalizedBeat);

                // Auto-release if holding at release time.
                if (normalizedBeat >= 1f)
                    endEvent.Hit(0f, 1f);
            }

            loop.transform.localScale = Vector2.one / holder.transform.localScale;
        }

        public void EndAce()
        {
            tweezers.LongPluck(true, this);
            tweezers.hitOnFrame++;

            if (pullSound != null)
                pullSound.Stop();

            pluckState = -1;
        }

        public void EndEarly()
        {
            var normalized = Conductor.instance.GetPositionFromBeat(createBeat + game.tweezerBeatOffset + game.beatInterval, 0.5f);
            anim.Play("LoopPullReverse", 0, normalized);
            tweezers.anim.Play("Idle", 0, 0);

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
            if (PlayerInput.GetAnyDirectionDown())
            {
                endInput = InputType.DIRECTION_UP;
            }
            else
            {
                endInput = InputType.STANDARD_UP;
            }
            pullSound = Jukebox.PlayOneShotGame($"rhythmTweezers/longPull{UnityEngine.Random.Range(1, 5)}");
            pluckState = 1;
            endEvent = game.ScheduleInput(createBeat, game.tweezerBeatOffset + game.beatInterval + 0.5f, endInput, EndJust, Out, Out);
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

        void OnDestroy()
        {
            if (pluckEvent != null)
                pluckEvent.Disable();
            if (endEvent != null)
                endEvent.Disable();
        }
    }
}