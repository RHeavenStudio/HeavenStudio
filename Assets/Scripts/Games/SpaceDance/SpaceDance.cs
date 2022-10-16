using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbSpaceDanceLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("spaceDance", "Space Dance \n<color=#eb5454>[WIP]</color>", "FFFF34", false, false, new List<GameAction>()
            {
                new GameAction("turn right", "Turn Right")
                {
                    function = delegate { SpaceDance.instance.DoTurnRight(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2.0f, 
                },
                new GameAction("sit down", "Sit Down")
                {
                    function = delegate { SpaceDance.instance.DoSitDown(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2.0f, 
                },
                new GameAction("punch", "Punch")
                {
                    function = delegate { SpaceDance.instance.DoPunch(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2.0f, 
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate { SpaceDance.instance.Bop(eventCaller.currentEntity.beat); },
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    // using Scripts_SpaceDance;
    public class SpaceDance : Minigame
    {
        public Animator DancerP;
        public Animator Dancer1;
        public Animator Dancer2;
        public Animator Dancer3;
        public Animator Gramps;
        public Animator Hit;
        public GameObject Player;

        public static SpaceDance instance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {

            if (PlayerInput.Pressed() && !IsExpectingInputNow())
            {
            Jukebox.PlayOneShotGame("spaceDance/inputBad");
            // Look at this later, sound effect has some weird clipping on it sometimes?? popping. like. fucking popopop idk why its doing that its fine theres no sample weirdness ughh
            }
        }

        public void DoTurnRight(float beat)
        {
            ScheduleInput(beat, 1f, InputType.DIRECTION_RIGHT_DOWN, RightSuccess, RightMiss, RightEmpty);
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("spaceDance/voicelessTurn", beat),
            new MultiSound.Sound("spaceDance/dancerTurn", beat),
            new MultiSound.Sound("spaceDance/dancerRight", beat + 1.0f),
            });

            BeatAction.New(Player, new List<BeatAction.Action>() 
                {
                new BeatAction.Action(beat,     delegate { DancerP.Play("TurnRightStart", -1, 0);}),
                new BeatAction.Action(beat,     delegate { Dancer1.Play("TurnRightStart", -1, 0);}),
                new BeatAction.Action(beat,     delegate { Dancer2.Play("TurnRightStart", -1, 0);}),
                new BeatAction.Action(beat,     delegate { Dancer3.Play("TurnRightStart", -1, 0);}),
                new BeatAction.Action(beat + 1f,     delegate { Dancer1.Play("TurnRightDo", -1, 0);}),
                new BeatAction.Action(beat + 1f,     delegate { Dancer2.Play("TurnRightDo", -1, 0);}),
                new BeatAction.Action(beat + 1f,     delegate { Dancer3.Play("TurnRightDo", -1, 0);}),
                });

        }

        public void DoSitDown(float beat)
        {
            ScheduleInput(beat, 1f, InputType.DIRECTION_DOWN_DOWN, SitSuccess, SitMiss, SitEmpty);
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("spaceDance/voicelessSit", beat),
            new MultiSound.Sound("spaceDance/dancerLets", beat),
            new MultiSound.Sound("spaceDance/dancerSit", beat + 0.5f),
            new MultiSound.Sound("spaceDance/dancerDown", beat + 1f),
            });

            BeatAction.New(Player, new List<BeatAction.Action>() 
                {
                new BeatAction.Action(beat,     delegate { DancerP.Play("SitDownStart", -1, 0);}),
                new BeatAction.Action(beat,     delegate { Dancer1.Play("SitDownStart", -1, 0);}),
                new BeatAction.Action(beat,     delegate { Dancer2.Play("SitDownStart", -1, 0);}),
                new BeatAction.Action(beat,     delegate { Dancer3.Play("SitDownStart", -1, 0);}),
                new BeatAction.Action(beat + 1f,     delegate { Dancer1.Play("SitDownDo", -1, 0);}),
                new BeatAction.Action(beat + 1f,     delegate { Dancer2.Play("SitDownDo", -1, 0);}),
                new BeatAction.Action(beat + 1f,     delegate { Dancer3.Play("SitDownDo", -1, 0);}),
                });

        }

        public void DoPunch(float beat)
        {
            ScheduleInput(beat, 1.5f, InputType.STANDARD_DOWN, PunchSuccess, PunchMiss, PunchEmpty);
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("spaceDance/voicelessPunch", beat),
            new MultiSound.Sound("spaceDance/dancerPa", beat),
            new MultiSound.Sound("spaceDance/voicelessPunch", beat + 0.5f),
            new MultiSound.Sound("spaceDance/dancerPa", beat + 0.5f),
            new MultiSound.Sound("spaceDance/voicelessPunch", beat + 1f),
            new MultiSound.Sound("spaceDance/dancerPa", beat + 1f),
            new MultiSound.Sound("spaceDance/dancerPunch", beat + 1.5f),
            });

            BeatAction.New(Player, new List<BeatAction.Action>() 
                {
                new BeatAction.Action(beat,     delegate { DancerP.Play("PunchStartInner", -1, 0);}),
                new BeatAction.Action(beat,     delegate { Dancer1.Play("PunchStartInner", -1, 0);}),
                new BeatAction.Action(beat,     delegate { Dancer2.Play("PunchStartInner", -1, 0);}),
                new BeatAction.Action(beat,     delegate { Dancer3.Play("PunchStartInner", -1, 0);}),
                new BeatAction.Action(beat + 0.5f,     delegate { DancerP.Play("PunchStartOuter", -1, 0);}),
                new BeatAction.Action(beat + 0.5f,     delegate { Dancer1.Play("PunchStartOuter", -1, 0);}),
                new BeatAction.Action(beat + 0.5f,     delegate { Dancer2.Play("PunchStartOuter", -1, 0);}),
                new BeatAction.Action(beat + 0.5f,     delegate { Dancer3.Play("PunchStartOuter", -1, 0);}),
                new BeatAction.Action(beat + 1f,     delegate { DancerP.Play("PunchStartInner", -1, 0);}),
                new BeatAction.Action(beat + 1f,     delegate { Dancer1.Play("PunchStartInner", -1, 0);}),
                new BeatAction.Action(beat + 1f,     delegate { Dancer2.Play("PunchStartInner", -1, 0);}),
                new BeatAction.Action(beat + 1f,     delegate { Dancer3.Play("PunchStartInner", -1, 0);}),
                new BeatAction.Action(beat + 1.5f,     delegate { Dancer1.Play("PunchDo", -1, 0);}),
                new BeatAction.Action(beat + 1.5f,     delegate { Dancer2.Play("PunchDo", -1, 0);}),
                new BeatAction.Action(beat + 1.5f,     delegate { Dancer3.Play("PunchDo", -1, 0);}),
                });

        }

        public void Bop(float beat)
        {
            BeatAction.New(Player, new List<BeatAction.Action>() 
                {
                new BeatAction.Action(beat,     delegate { DancerP.Play("Bop", -1, 0);}),
                new BeatAction.Action(beat,     delegate { Dancer1.Play("Bop", -1, 0);}),
                new BeatAction.Action(beat,     delegate { Dancer2.Play("Bop", -1, 0);}),
                new BeatAction.Action(beat,     delegate { Dancer3.Play("Bop", -1, 0);}),
                });

        }

        public void RightSuccess(PlayerActionEvent caller, float state)
            {
            Jukebox.PlayOneShotGame("spaceDance/inputGood");
            DancerP.Play("TurnRightDo", -1, 0);
             }

        public void RightMiss(PlayerActionEvent caller)
            {
            Jukebox.PlayOneShotGame("spaceDance/inputBad2");
            DancerP.Play("Ouch", -1, 0);
            Hit.Play("HitTurn", -1, 0);
             }

        public void RightEmpty(PlayerActionEvent caller)
            {

             }

        public void SitSuccess(PlayerActionEvent caller, float state)
            {
            Jukebox.PlayOneShotGame("spaceDance/inputGood");
            DancerP.Play("SitDownDo", -1, 0);
             }

        public void SitMiss(PlayerActionEvent caller)
            {
            Jukebox.PlayOneShotGame("spaceDance/inputBad2");
            DancerP.Play("Ouch", -1, 0);
            Hit.Play("HitSit", -1, 0);
             }

        public void SitEmpty(PlayerActionEvent caller)
            {

             }

        public void PunchSuccess(PlayerActionEvent caller, float state)
            {
            Jukebox.PlayOneShotGame("spaceDance/inputGood");
            DancerP.Play("PunchDo", -1, 0);
             }

        public void PunchMiss(PlayerActionEvent caller)
            {
            Jukebox.PlayOneShotGame("spaceDance/inputBad2");
            DancerP.Play("Ouch", -1, 0);
            Hit.Play("HitPunch", -1, 0);
             }

        public void PunchEmpty(PlayerActionEvent caller)
            {

             }


    }
}