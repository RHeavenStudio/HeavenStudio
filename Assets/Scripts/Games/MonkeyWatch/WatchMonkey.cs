using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;


namespace HeavenStudio.Games.Scripts_MonkeyWatch
{
    public class WatchMonkey : MonoBehaviour
    {
        private Animator anim;
        private Animator holeAnim;
        [Header("Properties")]
        [SerializeField] private bool isPink;

        private MonkeyWatch game;

        private int direction = 0;
        public double monkeyBeat;

        private double disappearBeat = 0;
        private bool disappear = false;

        private PlayerActionEvent inputEvent;

        private void Awake()
        {
            game = MonkeyWatch.instance;
            anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (disappear)
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(disappearBeat, 0.5f);
                anim.DoNormalizedAnimation(isPink ? "PinkAppear" : "Appear", 1 - normalizedBeat);
                float normalizedBeatClose = Conductor.instance.GetPositionFromBeat(disappearBeat + 0.25f, 0.25f);
                holeAnim.DoNormalizedAnimation("HoleClose", Mathf.Clamp01(normalizedBeatClose));
                if (normalizedBeat > 1f)
                {
                    Destroy(gameObject);
                }
            }
        }

        public void Appear(double beat, bool instant, Animator hole, int dir)
        {
            monkeyBeat = beat;
            direction = dir;
            holeAnim = hole;
            holeAnim.DoScaledAnimationAsync("HoleOpen", 0.4f, instant ? 1 : 0);
            anim.DoScaledAnimationAsync(isPink ? "PinkAppear" : "Appear", 0.4f, instant ? 1 : 0);
        }

        public void Disappear(double beat)
        {
            disappear = true;
            disappearBeat = beat;
            Update();
        }

        public void Prepare(double prepareBeat, double inputBeat)
        {
            anim.DoScaledAnimationAsync(isPink ? "PinkPrepare" + direction : "Prepare" + direction, 0);
            inputEvent = game.ScheduleInput(prepareBeat, inputBeat - prepareBeat, MonkeyWatch.InputAction_BasicPress, Just, Miss, Empty);
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(prepareBeat - 0.25, delegate
                {
                    if (inputEvent != null && inputEvent.enabled) anim.DoScaledAnimationAsync(isPink ? "PinkPrepare" + direction : "Prepare" + direction, 0.4f);
                })
            });
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            bool barely = state >= 1f || state <= -1f;
            if (barely)
            {
                SoundByte.PlayOneShot("nearMiss");
            }
            else
            {
                SoundByte.PlayOneShotGame(isPink ? "monkeyWatch/clapOffbeat" : $"monkeyWatch/clapOnbeat{UnityEngine.Random.Range(1, 6)}");
            }
            game.monkeyClockArrow.Move();
            game.PlayerMonkeyClap(isPink, barely);

            anim.DoScaledAnimationAsync(isPink ? "PinkClap" + direction : "Clap" + direction, 0.4f);
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.timer + caller.startBeat + 1, delegate
                {
                    string whichAnim = barely ? "Barely" : "Just";
                    anim.DoScaledAnimationAsync(isPink ? "Pink" + whichAnim : whichAnim, 0.4f);
                })
            });
        }

        private void Miss(PlayerActionEvent caller)
        {
            anim.DoScaledAnimationAsync(isPink ? "PinkMiss" : "Miss", 0.4f);
            game.monkeyClockArrow.Move();
        }

        private void Empty(PlayerActionEvent caller)
        {

        }
    }
}

