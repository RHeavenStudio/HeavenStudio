using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_CatchyTune
{
    public class Fruit : MonoBehaviour
    {

        public bool isPineapple;
        public float startBeat;

        public Animator anim;

        public bool side;

        public float barelyStart = 0f;

        public bool smile;

        public float endSmile;

        private string soundText;

        private Minigame.Eligible e = new Minigame.Eligible();

        private CatchyTune game;

        private float beatLength = 4f;

        private void Awake()
        {
            game = CatchyTune.instance;

            e.gameObject = this.gameObject;

            var cond = Conductor.instance;
            var tempo = cond.songBpm;
            var playbackSpeed = cond.musicSource.pitch;

            if (isPineapple) beatLength = 8f;

            if (side)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }

            anim.DoScaledAnimation("fruit bounce", startBeat, beatLength + (isPineapple ? 1f : 0.5f));

            soundText = "catchyTune/";

            if (side)
            {
                soundText += "right";
            }
            else
            {
                soundText += "left";
            }

            if (isPineapple)
            {
                soundText += "Pineapple";
            }
            else
            {
                soundText += "Orange";
            }

            game.ScheduleInput(startBeat, beatLength, side ? InputType.STANDARD_DOWN : InputType.DIRECTION_DOWN,
                CatchFruit, Miss, WayOff);
        }

        // minenice: note - needs PlayerActionEvent implementation
        private void Update()
        {
            if (barelyStart > 0f)
            {
                anim.DoScaledAnimation("barely", barelyStart, isPineapple ? 2f : 1f);
            }
            else 
            {
                anim.DoScaledAnimation("fruit bounce", startBeat, beatLength + (isPineapple ? 4f : 2f));
            }
        }

        public static void PlaySound(float startBeat, bool side, bool isPineapple)
        {
            string soundText = "catchyTune/";

            if (side)
            {
                soundText += "right";
            }
            else
            {
                soundText += "left";
            }

            if (isPineapple)
            {
                soundText += "Pineapple";
            }
            else
            {
                soundText += "Orange";
            }


            MultiSound.Sound[] sound;


            if (isPineapple)
            {
                sound = new MultiSound.Sound[]
                {
                    new MultiSound.Sound(soundText, startBeat + 2f),
                    new MultiSound.Sound(soundText, startBeat + 4f),
                    new MultiSound.Sound(soundText, startBeat + 6f)
                };
            }
            else
            {
                sound = new MultiSound.Sound[]
                {
                    new MultiSound.Sound(soundText, startBeat + 1f),
                    new MultiSound.Sound(soundText, startBeat + 2f),
                    new MultiSound.Sound(soundText, startBeat + 3f)
                };
            }

            MultiSound.Play(sound, forcePlay: true);
        }

        private void CatchFruit(PlayerActionEvent caller, float state)
        {

            if (state <= -1f || state >= 1f)
            {
                //near miss (barely)
                barelyStart = Conductor.instance.songPositionInBeats;
                
                game.catchBarely(side);

                // play near miss animation
                anim.DoScaledAnimation("barely", barelyStart, isPineapple ? 2f : 1f);

                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(barelyStart + (isPineapple ? 2f : 1f), delegate { Destroy(this.gameObject); }),
                });

            }
            else 
            {
                Jukebox.PlayOneShotGame(soundText + "Catch");
                game.catchSuccess(side, isPineapple, smile, startBeat + beatLength, endSmile);
                Destroy(this.gameObject);
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            game.catchMiss(side, isPineapple);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat + beatLength + (isPineapple ? 3f : 1.5f), delegate { Destroy(this.gameObject); }),
            });
        }

        private void WayOff(PlayerActionEvent caller) {} // whiffing is handled in the main loop
    }
}
