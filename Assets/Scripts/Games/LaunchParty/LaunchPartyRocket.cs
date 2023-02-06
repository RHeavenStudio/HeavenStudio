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

        void Awake()
        {
            anim = GetComponent<Animator>();
            numberAnim = number.GetComponent<Animator>();
            number.SetActive(false);
            game = LaunchParty.instance;
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

            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
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

            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
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

        void JustFamilyRocket(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            SuccessFamilyRocket(caller);
        }

        void SuccessFamilyRocket(PlayerActionEvent caller)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("launchParty/rocket_note", caller.startBeat + caller.timer, pitches[3]),
                new MultiSound.Sound("launchParty/rocket_family", caller.startBeat + caller.timer),
            }, forcePlay: true);
            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer, delegate { numberAnim.DoScaledAnimationAsync("CountImpact", 0.5f); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { GameObject.Destroy(this.gameObject); }),
            });
        }

        void JustPartyCracker(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            SuccessPartyCracker(caller);
        }

        void SuccessPartyCracker(PlayerActionEvent caller)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("launchParty/popper_note", caller.startBeat + caller.timer, pitches[5]),
                new MultiSound.Sound("launchParty/rocket_crackerblast", caller.startBeat + caller.timer),
            }, forcePlay: true);
            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer, delegate { numberAnim.DoScaledAnimationAsync("CountImpact", 0.5f); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { GameObject.Destroy(this.gameObject); }),
            });
        }

        void Miss(PlayerActionEvent caller)
        {

        }

        void Nothing(PlayerActionEvent caller) {}
    }
}


