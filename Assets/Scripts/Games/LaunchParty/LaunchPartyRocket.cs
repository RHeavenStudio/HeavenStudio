using HeavenStudio.Util;
using JetBrains.Annotations;
using Starpelly.Transformer;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using static HeavenStudio.EntityTypes;

namespace HeavenStudio.Games.Scripts_LaunchParty
{
    public class LaunchPartyRocket : PlayerActionObject
    {
        public List<float> pitches = new List<float>();
        [SerializeField] Animator anim;
        [SerializeField] GameObject number;
        Animator numberAnim;
        private LaunchParty game;
        private bool noInput;

        void Awake()
        {
            anim = GetComponent<Animator>();
            numberAnim = number.GetComponent<Animator>();
            number.SetActive(false);
            game = LaunchParty.instance;
            anim.Play("RocketHidden", 0, 0);
        }

        void Update()
        {
            if (PlayerInput.Pressed() && !game.IsExpectingInputNow(InputType.STANDARD_DOWN) && !noInput)
            {
                Jukebox.PlayOneShotGame("launchParty/miss");
                Jukebox.PlayOneShotGame("launchParty/rocket_endBad");
                string leftOrRight = (UnityEngine.Random.Range(1, 3) == 1) ? "Left" : "Right";
                if (!isPlaying(anim, "RocketBarelyLeft") && !isPlaying(anim, "RocketBarelyRight")) anim.Play("RocketBarely" + leftOrRight, 0, 0);
                game.ScoreMiss(0.5);
            }
        }

        public void Rise()
        {
            anim.DoScaledAnimationAsync("RocketRise", 0.5f);
        }

        public void InitFamilyRocket(float beat)
        {
            game.ScheduleInput(beat, 3f, InputType.STANDARD_DOWN, JustFamilyRocket, Miss, Nothing);

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("launchParty/rocket_prepare", beat),
                new MultiSound.Sound("launchParty/rocket_note", beat, pitches[0]),
                new MultiSound.Sound("launchParty/rocket_note", beat + 1, pitches[1]),
                new MultiSound.Sound("launchParty/rocket_note", beat + 2, pitches[2]),
            }, forcePlay: true);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    number.SetActive(true);
                    numberAnim.Play("CountThree", 0, 0);
                }),
                new BeatAction.Action(beat + 1, delegate { numberAnim.Play("CountTwo", 0, 0); }),
                new BeatAction.Action(beat + 2, delegate { numberAnim.Play("CountOne", 0, 0); }),
            });
        }

        public void InitPartyCracker(float beat)
        {
            game.ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, JustPartyCracker, Miss, Nothing);

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("launchParty/rocket_prepare", beat),
                new MultiSound.Sound("launchParty/popper_note", beat, pitches[0]),
                new MultiSound.Sound("launchParty/popper_note", beat + 0.66f, pitches[1]),
                new MultiSound.Sound("launchParty/popper_note", beat + 1, pitches[2]),
                new MultiSound.Sound("launchParty/popper_note", beat + 1.33f, pitches[3]),
                new MultiSound.Sound("launchParty/popper_note", beat + 1.66f, pitches[4]),
            }, forcePlay: true);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    number.SetActive(true);
                    numberAnim.Play("CountFive", 0, 0);
                }),
                new BeatAction.Action(beat + 0.66f, delegate { numberAnim.Play("CountFour", 0, 0); }),
                new BeatAction.Action(beat + 1, delegate { numberAnim.Play("CountThree", 0, 0); }),
                new BeatAction.Action(beat + 1.33f, delegate { numberAnim.Play("CountTwo", 0, 0); }),
                new BeatAction.Action(beat + 1.66f, delegate { numberAnim.Play("CountOne", 0, 0); }),
            });
        }

        public void InitBell(float beat)
        {
            game.ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, JustBell, Miss, Nothing);

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("launchParty/rocket_prepare", beat),
                new MultiSound.Sound("launchParty/bell_note", beat, pitches[0]),
                new MultiSound.Sound("launchParty/bell_short", beat + 1f, pitches[1]),
                new MultiSound.Sound("launchParty/bell_short", beat + 1.16f, pitches[2]),
                new MultiSound.Sound("launchParty/bell_short", beat + 1.33f, pitches[3]),
                new MultiSound.Sound("launchParty/bell_short", beat + 1.5f, pitches[4]),
                new MultiSound.Sound("launchParty/bell_short", beat + 1.66f, pitches[5]),
                new MultiSound.Sound("launchParty/bell_short", beat + 1.83f, pitches[6]),
            }, forcePlay: true);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    number.SetActive(true);
                    numberAnim.Play("CountSeven", 0, 0);
                }),
                new BeatAction.Action(beat + 1f, delegate { numberAnim.Play("CountSix", 0, 0); }),
                new BeatAction.Action(beat + 1.16f, delegate { numberAnim.Play("CountFive", 0, 0); }),
                new BeatAction.Action(beat + 1.33f, delegate { numberAnim.Play("CountFour", 0, 0); }),
                new BeatAction.Action(beat + 1.5f, delegate { numberAnim.Play("CountThree", 0, 0); }),
                new BeatAction.Action(beat + 1.66f, delegate { numberAnim.Play("CountTwo", 0, 0); }),
                new BeatAction.Action(beat + 1.83f, delegate { numberAnim.Play("CountOne", 0, 0); }),
            });
        }

        public void InitBowlingPin(float beat)
        {
            game.ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, JustBowlingPin, Miss, Nothing);

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("launchParty/rocket_pin_prepare", beat),
                new MultiSound.Sound("launchParty/pin", beat, pitches[0]),
                new MultiSound.Sound("launchParty/flute", beat, pitches[1], 0.02f),
                new MultiSound.Sound("launchParty/flute", beat + 0.16f, pitches[2], 0.02f),
                new MultiSound.Sound("launchParty/flute", beat + 0.33f, pitches[3], 0.06f),
                new MultiSound.Sound("launchParty/flute", beat + 0.5f, pitches[4], 0.1f),
                new MultiSound.Sound("launchParty/flute", beat + 0.66f, pitches[5], 0.16f),
                new MultiSound.Sound("launchParty/flute", beat + 0.83f, pitches[6], 0.22f),
                new MultiSound.Sound("launchParty/flute", beat + 1f, pitches[7], 0.3f),
                new MultiSound.Sound("launchParty/flute", beat + 1.16f, pitches[8], 0.4f),
                new MultiSound.Sound("launchParty/flute", beat + 1.33f, pitches[9], 0.6f),
                new MultiSound.Sound("launchParty/flute", beat + 1.5f, pitches[10], 0.75f),
                new MultiSound.Sound("launchParty/flute", beat + 1.66f, pitches[11], 0.89f),
                new MultiSound.Sound("launchParty/flute", beat + 1.83f, pitches[12]),
            }, forcePlay: true);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    number.SetActive(true);
                    numberAnim.Play("CountOne", 0, 0);
                }),
            });
        }

        void JustFamilyRocket(PlayerActionEvent caller, float state)
        {
            noInput = true;
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("launchParty/miss");
                Jukebox.PlayOneShotGame("launchParty/rocket_endBad");
                string leftOrRight = (UnityEngine.Random.Range(1, 3) == 1) ? "Left" : "Right";
                anim.Play("RocketBarely" + leftOrRight, 0, 0);
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { GameObject.Destroy(gameObject); }),
                });
                return;
            }
            SuccessFamilyRocket(caller);
        }

        void SuccessFamilyRocket(PlayerActionEvent caller)
        {
            game.launchPadSpriteAnim.DoScaledAnimationAsync("SizeUp", 1f);
            anim.Play("RocketLaunch", 0, 0);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("launchParty/rocket_note", caller.startBeat + caller.timer, pitches[3]),
                new MultiSound.Sound("launchParty/rocket_family", caller.startBeat + caller.timer),
            }, forcePlay: true);
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer, delegate { numberAnim.DoScaledAnimationAsync("CountImpact", 0.5f); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { GameObject.Destroy(gameObject); }),
            });
        }

        void JustPartyCracker(PlayerActionEvent caller, float state)
        {
            noInput = true;
            if (state >= 1f || state <= -1f)
            {
                string leftOrRight = (UnityEngine.Random.Range(1, 3) == 1) ? "Left" : "Right";
                anim.Play("RocketBarely" + leftOrRight, 0, 0);
                Jukebox.PlayOneShotGame("launchParty/miss");
                Jukebox.PlayOneShotGame("launchParty/rocket_endBad");
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { GameObject.Destroy(gameObject); }),
                });
                return;
            }
            SuccessPartyCracker(caller);
        }

        void SuccessPartyCracker(PlayerActionEvent caller)
        {
            game.launchPadSpriteAnim.DoScaledAnimationAsync("SizeUp", 1f);
            anim.Play("RocketLaunch", 0, 0);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("launchParty/popper_note", caller.startBeat + caller.timer, pitches[5]),
                new MultiSound.Sound("launchParty/rocket_crackerblast", caller.startBeat + caller.timer),
            }, forcePlay: true);
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer, delegate { numberAnim.DoScaledAnimationAsync("CountImpact", 0.5f); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { GameObject.Destroy(gameObject); }),
            });
        }

        void JustBell(PlayerActionEvent caller, float state)
        {
            noInput = true;
            if (state >= 1f || state <= -1f)
            {
                string leftOrRight = (UnityEngine.Random.Range(1, 3) == 1) ? "Left" : "Right";
                anim.Play("RocketBarely" + leftOrRight, 0, 0);
                Jukebox.PlayOneShotGame("launchParty/miss");
                Jukebox.PlayOneShotGame("launchParty/rocket_endBad");
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { GameObject.Destroy(gameObject); }),
                });
                return;
            }
            SuccessBell(caller);
        }

        void SuccessBell(PlayerActionEvent caller)
        {
            game.launchPadSpriteAnim.DoScaledAnimationAsync("SizeUp", 1f);
            anim.Play("RocketLaunch", 0, 0);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("launchParty/bell_note", caller.startBeat + caller.timer, pitches[7]),
                new MultiSound.Sound("launchParty/bell_blast", caller.startBeat + caller.timer),
            }, forcePlay: true);
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer, delegate { numberAnim.DoScaledAnimationAsync("CountImpact", 0.5f); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { GameObject.Destroy(gameObject); }),
            });
        }

        void JustBowlingPin(PlayerActionEvent caller, float state)
        {
            noInput = true;
            if (state >= 1f || state <= -1f)
            {
                string leftOrRight = (UnityEngine.Random.Range(1, 3) == 1) ? "Left" : "Right";
                anim.Play("RocketBarely" + leftOrRight, 0, 0);
                Jukebox.PlayOneShotGame("launchParty/miss");
                Jukebox.PlayOneShotGame("launchParty/rocket_endBad");
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { GameObject.Destroy(gameObject); }),
                });
                return;
            }
            SuccessBowlingPin(caller);
        }

        void SuccessBowlingPin(PlayerActionEvent caller)
        {
            game.launchPadSpriteAnim.DoScaledAnimationAsync("SizeUp", 1f);
            anim.Play("RocketLaunch", 0, 0);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("launchParty/flute", caller.startBeat + caller.timer, pitches[13], 0.89f),
                new MultiSound.Sound("launchParty/pin", caller.startBeat + caller.timer, pitches[14]),
                new MultiSound.Sound("launchParty/rocket_bowling", caller.startBeat + caller.timer),
            }, forcePlay: true);
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer, delegate { numberAnim.DoScaledAnimationAsync("CountImpact", 0.5f); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { GameObject.Destroy(gameObject); }),
            });
        }

        void Miss(PlayerActionEvent caller)
        {
            noInput = true;
            Jukebox.PlayOneShotGame("launchParty/miss");
            number.SetActive(false);
            anim.Play("RocketMiss", 0, 0);
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { GameObject.Destroy(gameObject); }),
            });
        }

        bool isPlaying(Animator anim, string stateName)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
                    anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                return true;
            else
                return false;
        }

        void Nothing(PlayerActionEvent caller) {}
    }
}


