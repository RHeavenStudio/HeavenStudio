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
            holeAnim.DoScaledAnimationAsync("HoleOpen", 0.5f);
            anim.DoScaledAnimationAsync(isPink ? "Pink" : "" + "Appear", 0.5f, instant ? 1 : 0);
        }

        public void Disappear()
        {
            holeAnim.DoScaledAnimationAsync("HoleClose", 0.5f);
            Destroy(gameObject);
        }

        public void Prepare(double prepareBeat, double inputBeat)
        {
            anim.DoScaledAnimationAsync(isPink ? "Pink" : "" + "Prepare" + direction, 0);
            inputEvent = game.ScheduleInput(prepareBeat, inputBeat - prepareBeat, InputType.STANDARD_DOWN, Just, Miss, Empty);
            BeatAction.New(gameObject, new List<BeatAction.Action>() 
            {
                new BeatAction.Action(prepareBeat, delegate
                {
                    anim.DoScaledAnimationAsync(isPink ? "Pink" : "" + "Prepare" + direction, 0.5f);
                })
            });
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            game.PlayerMonkeyClap(isPink, state >= 1f || state <= -1f);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
                return;
            }
            SoundByte.PlayOneShotGame(isPink ? "monkeyWatch/clapOffbeat" : $"monkeyWatch/clapOnbeat{UnityEngine.Random.Range(1, 6)}");
            anim.DoScaledAnimationAsync(isPink ? "Pink" : "" + "Clap" + direction, 0.5f);
        }

        private void Miss(PlayerActionEvent caller)
        {

        }

        private void Empty(PlayerActionEvent caller) { }
    }
}

