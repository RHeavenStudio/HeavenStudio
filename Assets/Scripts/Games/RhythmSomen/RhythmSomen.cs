using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class PcoSomenLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("rhythmSomen", "Rhythm Sōmen", "99CC34", false, false, new List<GameAction>()
            {
                new GameAction("crane (far)", "Far Crane")
                {
                    function = delegate { RhythmSomen.instance.DoFarCrane(eventCaller.currentEntity.beat); },
                    defaultLength = 4.0f,
                },
                new GameAction("crane (close)", "Close Crane")
                {
                    function = delegate { RhythmSomen.instance.DoCloseCrane(eventCaller.currentEntity.beat); },
                    defaultLength = 3.0f,
                },
                new GameAction("crane (both)", "Both Cranes")
                {
                    function = delegate { RhythmSomen.instance.DoBothCrane(eventCaller.currentEntity.beat); },
                    defaultLength = 4.0f,
                },
                new GameAction("offbeat bell", "Offbeat Warning")
                {
                    function = delegate { RhythmSomen.instance.DoBell(eventCaller.currentEntity.beat); },
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    // using Scripts_RhythmSomen;
    public class RhythmSomen : Minigame
    {
        public Animator SomenPlayer;
        public Animator FrontArm;
        public Animator EffectHit;
        public Animator EffectSweat;
        public Animator EffectExclam;
        public Animator EffectShock;
        public Animator CloseCrane;
        public Animator FarCrane;
        public GameObject Player;

        public GameEvent bop = new GameEvent();

        public static RhythmSomen instance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;
            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                SomenPlayer.Play("HeadBob", -1, 0);
            }

            if (PlayerInput.Pressed() && !IsExpectingInputNow())
            {
                Jukebox.PlayOneShotGame("rhythmSomen/somen_mistake");
                FrontArm.Play("ArmPluck", -1, 0);
                EffectSweat.Play("BlobSweating", -1, 0);
                ScoreMiss();
            }
        }

        public void DoFarCrane(float beat)
        {
            //Far Drop Multisound
            ScheduleInput(beat, 3f, InputType.STANDARD_DOWN, CatchSuccess, CatchMiss, CatchEmpty);
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("rhythmSomen/somen_lowerfar", beat),
            new MultiSound.Sound("rhythmSomen/somen_drop", beat + 1f),
            new MultiSound.Sound("rhythmSomen/somen_woosh", beat + 1.5f),
            });

            BeatAction.New(Player, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat,     delegate { FarCrane.Play("Drop", -1, 0);}),
                new BeatAction.Action(beat + 1.0f,     delegate { FarCrane.Play("Open", -1, 0);}),
                new BeatAction.Action(beat + 1.5f,     delegate { FarCrane.Play("Lift", -1, 0);}),
                });

        }

        public void DoCloseCrane(float beat)
        {
            //Close Drop Multisound
            ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, CatchSuccess, CatchMiss, CatchEmpty);
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("rhythmSomen/somen_lowerclose", beat),
            new MultiSound.Sound("rhythmSomen/somen_drop", beat + 1f),
            new MultiSound.Sound("rhythmSomen/somen_woosh", beat + 1.5f),
            });

            BeatAction.New(Player, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat,     delegate { CloseCrane.Play("DropClose", -1, 0);}),
                new BeatAction.Action(beat + 1.0f,     delegate { CloseCrane.Play("OpenClose", -1, 0);}),
                new BeatAction.Action(beat + 1.5f,     delegate { CloseCrane.Play("LiftClose", -1, 0);}),
                });

        }

        public void DoBothCrane(float beat)
        {
            //Both Drop Multisound
            ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, CatchSuccess, CatchMiss, CatchEmpty);
            ScheduleInput(beat, 3f, InputType.STANDARD_DOWN, CatchSuccess, CatchMiss, CatchEmpty);
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("rhythmSomen/somen_lowerfar", beat),
            new MultiSound.Sound("rhythmSomen/somen_doublealarm", beat),
            new MultiSound.Sound("rhythmSomen/somen_drop", beat + 1f),
            new MultiSound.Sound("rhythmSomen/somen_woosh", beat + 1.5f),
            });

            BeatAction.New(Player, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat,     delegate { CloseCrane.Play("DropClose", -1, 0);}),
                new BeatAction.Action(beat,     delegate { FarCrane.Play("Drop", -1, 0);}),
                new BeatAction.Action(beat + 1.0f,     delegate { CloseCrane.Play("OpenClose", -1, 0);}),
                new BeatAction.Action(beat + 1.0f,     delegate { FarCrane.Play("Open", -1, 0);}),
                new BeatAction.Action(beat + 1.5f,     delegate { CloseCrane.Play("LiftClose", -1, 0);}),
                new BeatAction.Action(beat + 1.5f,     delegate { FarCrane.Play("Lift", -1, 0);}),
                });

        }

        public void DoBell(float beat)
        {
            //Bell Sound lol
            Jukebox.PlayOneShotGame("rhythmSomen/somen_bell");

            BeatAction.New(Player, new List<BeatAction.Action>()
                    {
                    new BeatAction.Action(beat,     delegate { EffectExclam.Play("ExclamAppear", -1, 0);}),
                    });

        }

        public void CatchSuccess(PlayerActionEvent caller, float state)
        {
            Jukebox.PlayOneShotGame("rhythmSomen/somen_catch");
            FrontArm.Play("ArmPluck", -1, 0);
            EffectHit.Play("HitAppear", -1, 0);
        }

        public void CatchMiss(PlayerActionEvent caller)
        {
            EffectShock.Play("ShockAppear", -1, 0);
        }

        public void CatchEmpty(PlayerActionEvent caller)
        {

        }
    }
}