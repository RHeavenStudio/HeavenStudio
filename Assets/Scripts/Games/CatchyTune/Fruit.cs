using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_CatchyTune
{
    public class Fruit : PlayerActionObject
    {

        public bool isPineapple;
        public float startBeat;

        public Animator anim;

        public bool side;

        public bool smile;

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
            Conductor cond = Conductor.instance;
            float tempo = cond.songBpm;
            float playbackSpeed = cond.musicSource.pitch;

            anim.DoScaledAnimation("fruit bounce", startBeat, beatLength + (isPineapple ? 1f : 0.5f));
            
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, beatLength);
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
            //minenice: TODO - near misses (-1 > state > 1)
            Jukebox.PlayOneShotGame(soundText + "Catch");
            game.catchSuccess(side, isPineapple, smile, startBeat + beatLength);
            Destroy(this.gameObject);
        }

        private void Miss(PlayerActionEvent caller)
        {
            game.catchMiss(side, isPineapple);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat + beatLength + (isPineapple ? 1f : 0.5f), delegate { Destroy(this.gameObject); }),
            });
        }

        private void WayOff(PlayerActionEvent caller) {} // whiffing is handled in the main loop
    }
}
