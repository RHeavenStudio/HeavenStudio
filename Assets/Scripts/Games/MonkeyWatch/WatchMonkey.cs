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

        private PlayerActionEvent inputEvent;

        private void Awake()
        {
            game = MonkeyWatch.instance;
            anim = GetComponent<Animator>();
        }

        public void Appear(double beat, bool instant, Animator hole, int dir)
        {
            monkeyBeat = beat;
            direction = dir;
            holeAnim = hole;
            holeAnim.DoScaledAnimationAsync("HoleOpen", 0.4f, instant ? 1 : 0);
            anim.DoScaledAnimationAsync(isPink ? "PinkAppear" : "Appear", 0.4f, instant ? 1 : 0);
        }

        public void Disappear()
        {
            holeAnim.DoScaledAnimationAsync("HoleClose", 0.4f);
            Destroy(gameObject);
        }

        public void Prepare(double prepareBeat, double inputBeat)
        {
            anim.DoScaledAnimationAsync(isPink ? "PinkPrepare" + direction : "Prepare" + direction, 0);
            inputEvent = game.ScheduleInput(prepareBeat, inputBeat - prepareBeat, InputType.STANDARD_DOWN, Just, Miss, Empty);
            BeatAction.New(gameObject, new List<BeatAction.Action>() 
            {
                new BeatAction.Action(prepareBeat, delegate
                {
                    anim.DoScaledAnimationAsync(isPink ? "PinkPrepare" + direction : "Prepare" + direction, 0.4f);
                })
            });
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            bool barely = state >= 1f || state <= -1f;
            if (barely)
            {
                SoundByte.PlayOneShot("miss");
            }
            else
            {
                SoundByte.PlayOneShotGame(isPink ? "monkeyWatch/clapOffbeat" : $"monkeyWatch/clapOnbeat{UnityEngine.Random.Range(1, 6)}");
            }
            
            game.PlayerMonkeyClap(isPink, barely);
            anim.DoScaledAnimationAsync(isPink ? "PinkClap" + direction : "Clap" + direction, 0.4f);
            BeatAction.New(gameObject, new List<BeatAction.Action>()
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
        }

        private void Empty(PlayerActionEvent caller) { }
    }
}

