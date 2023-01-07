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

        public bool eligable = true;

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

            anim.SetFloat("speed", GetAnimSpeed(isPineapple, tempo, playbackSpeed));

            if (side)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }

            anim.Play("fruit bounce", 0, 0f);

            soundText = "catchyTune/";

            if (side)
            {
                soundText += "right";
            }
            else {
                soundText += "left";
            }

            if (isPineapple)
            {
                soundText += "Pineapple";
            }
            else {
                soundText += "Orange";
            }


            MultiSound.Sound[] sound;


            if (isPineapple) {
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

            MultiSound.Play(sound);
        }

        private void Update()
        {
            Conductor cond = Conductor.instance;
            float tempo = cond.songBpm;
            float playbackSpeed = cond.musicSource.pitch;

            if (cond.isPaused)
            {
                anim.SetFloat("speed", 0f);
            }
            else {
                anim.SetFloat("speed", GetAnimSpeed(isPineapple, tempo, playbackSpeed));
            }

            

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, beatLength);

            if (eligable)
            {
                // check input timing
                StateCheck(normalizedBeat);
                bool pressed = (PlayerInput.Pressed() && side) || (PlayerInput.GetAnyDirectionDown() && !side);
                if (pressed)
                {
                    if (state.perfect)
                    {
                        CatchFruit();
                    }
                    else if (state.notPerfect())
                    {
                        Miss();
                    }
                    else {
                        WayOff();
                    }
                }
            }

            // fell off screen
            if (normalizedBeat > 1.5f) {
                Destroy(this.gameObject);
            }
        }

        private float GetAnimSpeed(bool pineapple, float tempo, float playbackSpeed)
        {
            float speedmult = pineapple ? 0.5f : 1f;
            return (speedmult * tempo / 60f) * 0.17f * playbackSpeed;
        }

        public override void OnAce()
        {
            CatchFruit();
        }

        private void CatchFruit()
        {
            //print("catch fruit");
            Jukebox.PlayOneShotGame(soundText + "Catch");
            game.catchSuccess(side, isPineapple, startBeat+beatLength);
            Destroy(this.gameObject);
        }

        private void Miss()
        {
            //print("miss fruit");
            eligable = false;
            game.catchMiss(side, isPineapple);
            Jukebox.PlayOneShotGame("catchyTune/missTest");
        }

        private void WayOff()
        {
            //print("way off");
            //eligable = false;
            Jukebox.PlayOneShotGame("catchyTune/whiff");
        }
    }
}
