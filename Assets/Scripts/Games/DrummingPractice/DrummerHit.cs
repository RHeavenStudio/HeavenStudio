using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DrummingPractice
{
    public class DrummerHit : MonoBehaviour
    {
        DrummingPractice game;
        public double startBeat;
        public bool applause = true;

        // Start is called before the first frame update
        void Awake()
        {
            game = DrummingPractice.instance;
        }

        void Start() 
        { 
            game.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN, Just, Miss, Out);

            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat+1f, delegate { 
                    SoundByte.PlayOneShotGame("drummingPractice/drum");
                    game.leftDrummer.Hit(true, false);
                    game.rightDrummer.Hit(true, false);
                }),
            });
        }

        // Update is called once per frame
        void Update() { }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f) {
                Hit(false);
            }
            Hit(true);
        }

        private void Miss(PlayerActionEvent caller) 
        {
            game.SetFaces(2);
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat+2f, delegate { 
                    game.SetFaces(0);
                })
            });
            CleanUp();
        }

        private void Out(PlayerActionEvent caller) {}

        public void Hit(bool _hit)
        {
            game.player.Hit(_hit, applause, true);
            game.SetFaces(_hit ? 1 : 2);

            if (!_hit)
            {
                BeatAction.New(game, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(startBeat+2f, delegate { 
                        game.SetFaces(0);
                    }),
                });
            }
            CleanUp();
        }

        public void CleanUp()
        {
            Destroy(gameObject);
        }
    }
}