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
            return new Minigame("rhythmSomen", "Rhythm Sōmen", "7ab96e", false, false, new List<GameAction>()
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
                new GameAction("slurp", "Slurp")
                {
                    function = delegate { RhythmSomen.instance.Slurp(eventCaller.currentEntity.beat); }
                },
                new GameAction("bop", "Bop") 
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmSomen.instance.ToggleBop(e.beat, e.length, e["toggle2"], e["toggle"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle2", true, "Bop", "Should the somen man bop?"),
                        new Param("toggle", false, "Bop (Auto)", "Should the somen man bop automatically?")
                    }
                }
            },
            new List<string>() {"pco", "normal"},
            "pcosomen", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    // using Scripts_RhythmSomen;
    public class RhythmSomen : Minigame
    {
        [SerializeField] ParticleSystem splashEffect;
        public Animator SomenPlayer;
        public Animator FrontArm;
        [SerializeField] Animator backArm;
        public Animator EffectHit;
        public Animator EffectSweat;
        public Animator EffectExclam;
        public Animator EffectShock;
        public Animator CloseCrane;
        public Animator FarCrane;
        public GameObject Player;
        private bool shouldBop = true;
        private bool missed;
        private bool hasSlurped;

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
            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1) && shouldBop)
            {
                SomenPlayer.Play("HeadBob", -1, 0);
            }

            if (PlayerInput.Pressed() && !IsExpectingInputNow())
            {
                SoundByte.PlayOneShotGame("rhythmSomen/somen_mistake");
                FrontArm.Play("ArmPluck", -1, 0);
                backArm.Play("BackArmNothing", 0, 0);
                hasSlurped = false;
                EffectSweat.Play("BlobSweating", -1, 0);
                ScoreMiss();
            }
        }

        public void Slurp(double beat)
        {
            if (!missed)
            {
                backArm.Play("BackArmLift", 0, 0);
                FrontArm.DoScaledAnimationAsync("ArmSlurp", 0.5f);
                hasSlurped = true;
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 1f, delegate
                    {
                        if (hasSlurped)
                        {
                            backArm.Play("BackArmNothing", 0, 0);
                            FrontArm.Play("ArmNothing", 0, 0);
                        }
                    })
                });
            }
        }

        public void ToggleBop(double beat, float length, bool bopOrNah, bool autoBop)
        {
            shouldBop = autoBop;
            if (bopOrNah)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            SomenPlayer.Play("HeadBob", -1, 0);
                        })
                    });
                }
            }
        }

        public void DoFarCrane(double beat)
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

        public void DoCloseCrane(double beat)
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

        public void DoBothCrane(double beat)
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

        public void DoBell(double beat)
        {
            //Bell Sound lol
            SoundByte.PlayOneShotGame("rhythmSomen/somen_bell");

            BeatAction.New(Player, new List<BeatAction.Action>()
                    {
                    new BeatAction.Action(beat,     delegate { EffectExclam.Play("ExclamAppear", -1, 0);}),
                    });

        }

        public void CatchSuccess(PlayerActionEvent caller, float state)
        {
            backArm.Play("BackArmNothing", 0, 0);
            hasSlurped = false;
            splashEffect.Play();
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("rhythmSomen/somen_splash");
                FrontArm.Play("ArmPluckNG", -1, 0);
                EffectSweat.Play("BlobSweating", -1, 0);
                missed = true;
                return;
            }
            SoundByte.PlayOneShotGame("rhythmSomen/somen_catch");
            SoundByte.PlayOneShotGame("rhythmSomen/somen_catch_old", volume: 0.25f);
            FrontArm.Play("ArmPluckOK", -1, 0);
            EffectHit.Play("HitAppear", -1, 0);
            missed = false;
        }

        public void CatchMiss(PlayerActionEvent caller)
        {
            missed = true;
            EffectShock.Play("ShockAppear", -1, 0);
        }

        public void CatchEmpty(PlayerActionEvent caller)
        {

        }
    }
}