using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering; //don't ask

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrIdolLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("fanClub", "Fan Club", "ff78ff", false, false, new List<GameAction>()
                {
                    new GameAction("bop", "Bop")
                    {
                        function = delegate { var e = eventCaller.currentEntity; FanClub.instance.Bop(e.beat, e.length, e["type"], e["type2"]); },
                        resizable = true,
                        parameters = new List<Param>()
                        {
                            new Param("type", FanClub.IdolBopType.Both, "Bop", "Set the character(s) to bop for the duration of this event."),
                            new Param("type2", FanClub.IdolBopType.None, "Bop", "Set the character(s) to automatically bop until another Bop event is reached."),
                        }
                    },
                    new GameAction("yeah, yeah, yeah", "Yeah, Yeah, Yeah!")
                    {
                        function = delegate { var e = eventCaller.currentEntity; FanClub.instance.CallHai(e.beat, e["toggle"], e["toggle2"]); },
                        defaultLength = 8,
                        parameters = new List<Param>()
                        {
                            new Param("toggle", false, "Mute Arisa", "Toggle if Arisa's cue should be muted."),
                            new Param("toggle2", false, "Mute Monkey SFX", "Toggle if the monkey's (including the player's) sound effects should be muted.")
                        },
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; FanClub.WarnHai(e.beat, e["toggle"]);},
                        preFunction = delegate { var e = eventCaller.currentEntity; FanClub.HaiSound(e.beat, e["toggle"], e["toggle2"]); }
                    },
                    new GameAction("I suppose", "I Suppose!")
                    {
                        function = delegate { var e = eventCaller.currentEntity; FanClub.instance.CallKamone(e.beat, e["toggle"], e["toggle2"], 0, e["type"], e["alt"]); },
                        defaultLength = 6,
                        parameters = new List<Param>()
                        {
                            new Param("type", FanClub.KamoneResponseType.Through, "Response type", "Type of response to use"),
                            new Param("toggle", false, "Mute Arisa", "Toggle if Arisa's cue should be muted."),
                            new Param("toggle2", false, "Mute Monkey SFX", "Toggle if the monkey's (including the player's) sound effects should be muted."),
                            new Param("alt", false, "Alternate cue", "Toggle if Arisa should use the \"Wonderful\" (iina) cue.")
                        },
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; FanClub.WarnKamone(e.beat, e["toggle"], 0, e["type"], e["alt"]);},
                        preFunction = delegate { var e = eventCaller.currentEntity; FanClub.KamoneSound(e.beat, e["toggle"], e["toggle2"], 0, e["type"], e["alt"]); }
                    },
                    new GameAction("double clap", "Double Clap")
                    {
                        function = delegate { var e = eventCaller.currentEntity; FanClub.instance.CallBigReady(e.beat, e["toggle"]); },
                        defaultLength = 4,
                        parameters = new List<Param>()
                        {
                            new Param("toggle", false, "Disable Call", "Toggle if the monkey's \"Ooh!\" cue should play.")
                        },
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; FanClub.WarnBigReady(e.beat, e["toggle"]); },
                        preFunction = delegate { var e = eventCaller.currentEntity; FanClub.BigReadySound(e.beat, e["toggle"]); }
                    },
                    new GameAction("play idol animation", "Idol Coreography")
                    {
                        function = delegate { var e = eventCaller.currentEntity; FanClub.instance.PlayAnim(e.beat, e.length, e["type"], e["who"]); },
                        resizable = true,
                        parameters = new List<Param>()
                        {
                            new Param("type", FanClub.IdolAnimations.Bop, "Animation", "Set the animation to play."),
                            new Param("who", FanClub.IdolType.All, "Target Idol", "Set the character to perform the above animation.")
                        }
                    },
                    new GameAction("play stage animation", "Stage Effects")
                    {
                        function = delegate { var e = eventCaller.currentEntity; FanClub.instance.PlayAnimStage(e.beat, e["type"]); },
                        resizable = true,
                        parameters = new List<Param>()
                        {
                            new Param("type", FanClub.StageAnimations.Flash, "Effect", "Set the effect to play.")
                        }
                    },
                    new GameAction("friend walk", "Backup Dancers")
                    {
                        function = delegate { var e = eventCaller.currentEntity; FanClub.instance.DancerTravel(e.beat, e.length, e["exit"], e["instant"]); },
                        defaultLength = 16f,
                        resizable = true,
                        parameters = new List<Param>()
                        {
                            new Param("exit", false, "Exit", "Toggle if the backup dancers should enter or exit the scene."),
                            new Param("instant", false, "Instant", "Toggle if the backup dancers should instantly finish their enter/exit."),
                        }
                    },
                    new GameAction("set performance type", "Coreography Type")
                    {

                        function = delegate { var e = eventCaller.currentEntity; FanClub.SetPerformanceType(e["type"]);},
                        defaultLength = 0.5f,
                        parameters = new List<Param>()
                        {
                            new Param("type", FanClub.IdolPerformanceType.Normal, "Performance Type", "Set the type animations for Arisa to use.")
                        },
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; FanClub.SetPerformanceType(e["type"]); }
                    },
                    new GameAction("finish", "Applause")
                    {
                        function = delegate { var e = eventCaller.currentEntity; FanClub.instance.FinalCheer(e.beat); },
                    },
                },
                new List<string>() { "ntr", "normal" },
                "ntridol", "jp",
                new List<string>() { "jp" }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_FanClub;

    public class FanClub : Minigame
    {
        public enum IdolBopType
        {
            Both,
            Idol,
            Spectators,
            None
        }
        public enum IdolAnimations
        {
            Bop,
            PeaceVocal,
            Peace,
            Clap,
            Call,
            Response,
            Jump,
            BigCall,
            Squat,
            Wink,
            Dab,
            None
        }
        public enum KamoneResponseType
        {
            Through,
            Jump,
            ThroughFast,
            JumpFast,
        }
        public enum StageAnimations
        {
            Reset,
            Flash,
            Spot
        }
        public enum IdolPerformanceType
        {
            Normal,
            Arrange,
            // Tour(this one is fan made so ?)
        }
        public enum IdolType
        {
            All,
            Idol,
            LeftDancer,
            RightDancer
        }

        // userdata here
        [Header("Animators")]
        //stage
        [SerializeField] Animator StageAnimator;

        [Header("Objects")]
        // our girl
        [SerializeField] GameObject Arisa;
        [SerializeField] GameObject ArisaRootMotion;
        [SerializeField] GameObject ArisaShadow;

        // spectators
        [SerializeField] GameObject spectator;
        [SerializeField] GameObject spectatorAnchor;

        // backup dancers
        [SerializeField] NtrIdolAmie Blue;
        [SerializeField] NtrIdolAmie Orange;

        [Header("References")]
        [SerializeField] Material spectatorMat;

        // end userdata

        public bool JudgementPaused { get => noJudgement; }
        public bool JudgementInputPaused { get => noJudgementInput; set => noJudgementInput = value; }

        //arisa's animation controller
        private Animator idolAnimator;

        // blue's animation controller
        private Animator backupRAnimator;

        // orange's animation controller
        private Animator backupLAnimator;

        //spectators
        private NtrIdolFan Player;
        private List<GameObject> Spectators;

        //bop-type animations
        private GameEvent noBop = new GameEvent();
        private GameEvent noResponse = new GameEvent();
        private GameEvent noCall = new GameEvent();
        private GameEvent noSpecBop = new GameEvent();

        private double idolJumpStartTime = double.MinValue;
        private static int performanceType = (int)IdolPerformanceType.Normal;
        private bool responseToggle = false;
        private static double wantHais = double.MinValue;
        private static double wantKamone = double.MinValue;
        private static int wantKamoneType = (int)KamoneResponseType.Through;
        private static bool wantKamoneAlt = false;
        private static double wantBigReady = double.MinValue;
        private bool hasJumped = false;
        private bool noJudgement = false;
        private bool noJudgementInput = false;

        //game scene
        public static FanClub instance;

        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("NtrIdolTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);

        const int FAN_COUNT = 12;
        const float RADIUS = 1.5f;
        private void Awake()
        {
            instance = this;
            SetupBopRegion("fanClub", "bop", "type2", false);
            AddBopRegionEventsInt("fanClub", "finish", 3);
            Spectators = new List<GameObject>();
            idolAnimator = Arisa.GetComponent<Animator>();
            backupRAnimator = Blue.GetComponent<Animator>();
            backupLAnimator = Orange.GetComponent<Animator>();

            // procedurally spawning the spectators
            // from middle of viewport:

            //  |========A========|
            //  f==f==f==0==p==f==f
            // f==f==f==0==0==f==f==f

            //spawn 12, the 4th is our player (idx 3)
            Vector3 origin = spectatorAnchor.transform.localPosition;
            int sortOrigin = spectatorAnchor.GetComponent<SortingGroup>().sortingOrder;
            Vector3 spawnPos = new Vector3(origin.x, origin.y, origin.z);
            spawnPos.x -= RADIUS * 2 * 3;
            for (int i = 0; i < FAN_COUNT; i++)
            {
                GameObject mobj = Instantiate(spectator, spectatorAnchor.transform.parent);
                NtrIdolFan fan = mobj.GetComponent<NtrIdolFan>();
                mobj.transform.localPosition = new Vector3(spawnPos.x, spawnPos.y, spawnPos.z);
                mobj.GetComponent<SortingGroup>().sortingOrder = i + sortOrigin;
                if (i == 3)
                {
                    Player = fan;
                    Player.player = true;
                }
                Spectators.Add(mobj);

                //prepare spawn point of next spectator
                spawnPos.x += RADIUS * 2;
                if (i == 2)
                    spawnPos.x += RADIUS * 2;
                if (i == 8)
                    spawnPos.x += RADIUS * 4;
                if (i == 5)
                {
                    spawnPos = new Vector3(origin.x, origin.y, origin.z);
                    spawnPos.x -= RADIUS * 2 * 4 - RADIUS;
                    spawnPos.y -= RADIUS;
                    // spawnPos.z -= RADIUS/4;
                }
            }

            if (performanceType != (int)IdolPerformanceType.Normal)
            {
                idolAnimator.Play("NoPose" + GetPerformanceSuffix(), -1, 0);
            }

            ToSpot();
            noJudgement = false;
            noJudgementInput = false;
        }

        private void Start()
        {
            Blue.Init();
            Orange.Init();

            var amieWalkEvts = EventCaller.GetAllInGameManagerList("fanClub", new string[] { "friend walk" });
            foreach (var e in amieWalkEvts)
            {
                if (e.beat <= Conductor.instance.songPositionInBeatsAsDouble)
                {
                    DancerTravel(e.beat, e.length, e["exit"], e["instant"]);
                }
            }

            FanClub.SetPerformanceType((int)IdolPerformanceType.Normal);
            var choreoTypeEvts = EventCaller.GetAllInGameManagerList("fanClub", new string[] { "set performance type" });
            foreach (var e in choreoTypeEvts)
            {
                if (e.beat <= Conductor.instance.songPositionInBeatsAsDouble)
                {
                    FanClub.SetPerformanceType(e["type"]);
                }
            }
        }

        public static string GetPerformanceSuffix()
        {
            switch (performanceType)
            {
                case (int)IdolPerformanceType.Arrange:
                    return "Arrange";
                default:
                    return "";
            }
        }

        public static void SetPerformanceType(int type = (int)IdolPerformanceType.Normal)
        {
            performanceType = type;
            if (GameManager.instance.currentGame == "fanClub")
            {
                FanClub.instance.idolAnimator.Play("NoPose" + GetPerformanceSuffix(), -1, 0);
            }
        }

        public override void OnGameSwitch(double beat)
        {
            if (wantHais != double.MinValue)
            {
                ContinueHais(wantHais);
                wantHais = double.MinValue;
            }
            if (wantKamone != double.MinValue)
            {
                ContinueKamone(wantKamone, 0, wantKamoneType, wantKamoneAlt);
                wantKamone = double.MinValue;
            }
            if (wantBigReady != double.MinValue)
            {
                ContinueBigReady(wantBigReady);
                wantBigReady = double.MinValue;
            }
        }

        public override void OnBeatPulse(double beat)
        {
            var cond = Conductor.instance;
            int whoBops = BeatIsInBopRegionInt(beat);
            bool goBopIdol = whoBops == (int)IdolBopType.Both || whoBops == (int)IdolBopType.Idol;
            bool goBopSpec = whoBops == (int)IdolBopType.Both || whoBops == (int)IdolBopType.Spectators;
            if (goBopIdol)
            {
                if (!(cond.songPositionInBeatsAsDouble >= noBop.startBeat && cond.songPositionInBeatsAsDouble < noBop.startBeat + noBop.length))
                {
                    idolAnimator.Play("IdolBeat" + GetPerformanceSuffix(), 0, 0);
                    Blue.PlayAnimState("Beat");
                    Orange.PlayAnimState("Beat");
                }
            }
            if (goBopSpec)
            {
                if (!(cond.songPositionInBeatsAsDouble >= noSpecBop.startBeat && cond.songPositionInBeatsAsDouble < noSpecBop.startBeat + noSpecBop.length))
                    BopAll();
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;

            //idol jumping physics
            float jumpPos = cond.GetPositionFromBeat(idolJumpStartTime, 1f);
            float IDOL_SHADOW_SCALE = 1.18f;
            if (cond.songPositionInBeatsAsDouble >= idolJumpStartTime && cond.songPositionInBeatsAsDouble < idolJumpStartTime + 1f)
            {
                hasJumped = true;
                float yMul = jumpPos * 2f - 1f;
                float yWeight = -(yMul * yMul) + 1f;
                ArisaRootMotion.transform.localPosition = new Vector3(0, 2f * yWeight + 0.25f);
                ArisaShadow.transform.localScale = new Vector3((1f - yWeight * 0.8f) * IDOL_SHADOW_SCALE, (1f - yWeight * 0.8f) * IDOL_SHADOW_SCALE, 1f);
            }
            else
            {
                idolJumpStartTime = double.MinValue;
                ArisaRootMotion.transform.localPosition = new Vector3(0, 0);
                ArisaShadow.transform.localScale = new Vector3(IDOL_SHADOW_SCALE, IDOL_SHADOW_SCALE, 1f);
            }
        }

        public void Bop(double beat, float length, int target = (int)IdolBopType.Both, int targetAuto = (int)IdolBopType.Both)
        {
            for (int i = 0; i < length; i++)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + i, delegate { BopSingle(target); })
                });
            }
        }

        void BopSingle(int target)
        {
            switch (target)
            {
                case (int)IdolBopType.Idol:
                    idolAnimator.Play("IdolBeat" + GetPerformanceSuffix(), 0, 0);
                    Blue.PlayAnimState("Beat");
                    Orange.PlayAnimState("Beat");
                    break;
                case (int)IdolBopType.Spectators:
                    BopAll();
                    break;
                case (int)IdolBopType.Both:
                    idolAnimator.Play("IdolBeat" + GetPerformanceSuffix(), 0, 0);
                    Blue.PlayAnimState("Beat");
                    Orange.PlayAnimState("Beat");
                    BopAll();
                    break;
                default:
                    break;
            }
        }

        private void DisableBop(double beat, float length)
        {
            noBop.length = length;
            noBop.startBeat = beat;
        }

        private void DisableResponse(double beat, float length)
        {
            noResponse.length = length;
            noResponse.startBeat = beat;
        }

        private void DisableCall(double beat, float length)
        {
            noCall.length = length;
            noCall.startBeat = beat;
        }

        private void DisableSpecBop(double beat, float length)
        {
            double bt = Conductor.instance.songPositionInBeatsAsDouble;
            if (bt >= noSpecBop.startBeat && bt < noSpecBop.startBeat + noSpecBop.length)
            {
                double thisStToNextSt = beat - noSpecBop.startBeat;
                double newLen = thisStToNextSt + length;
                if (newLen > noSpecBop.length)
                    noSpecBop.length = (float)thisStToNextSt + length;
            }
            else
            {
                noSpecBop.length = length;
                noSpecBop.startBeat = beat;
            }
        }

        public void PlayAnim(double beat, float length, int type, int who)
        {
            idolJumpStartTime = double.MinValue;
            DisableResponse(beat, length + 0.5f);
            DisableBop(beat, length + 0.5f);
            DisableCall(beat, length + 0.5f);

            if (who is (int)IdolType.All or (int)IdolType.LeftDancer)
                Orange.PlayAnim(beat, length, type);
            if (who is (int)IdolType.All or (int)IdolType.RightDancer)
                Blue.PlayAnim(beat, length, type);

            if (who is (int)IdolType.All or (int)IdolType.Idol)
            {
                switch (type)
                {
                    case (int)IdolAnimations.Bop:
                        idolAnimator.Play("IdolBeat" + GetPerformanceSuffix(), -1, 0);
                        break;
                    case (int)IdolAnimations.PeaceVocal:
                        idolAnimator.Play("IdolPeace" + GetPerformanceSuffix(), -1, 0);
                        break;
                    case (int)IdolAnimations.Peace:
                        idolAnimator.Play("IdolPeaceNoSync" + GetPerformanceSuffix(), -1, 0);
                        break;
                    case (int)IdolAnimations.Clap:
                        idolAnimator.Play("IdolCrap" + GetPerformanceSuffix(), -1, 0);
                        break;
                    case (int)IdolAnimations.Call:
                        BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat,             delegate { Arisa.GetComponent<Animator>().Play("IdolCall0" + GetPerformanceSuffix(), -1, 0); }),
                        new BeatAction.Action(beat + 0.75f,     delegate { Arisa.GetComponent<Animator>().Play("IdolCall1" + GetPerformanceSuffix(), -1, 0); }),
                    });
                        break;
                    case (int)IdolAnimations.Response:
                        idolAnimator.Play("IdolResponse" + GetPerformanceSuffix(), -1, 0);
                        break;
                    case (int)IdolAnimations.Jump:
                        DoIdolJump(beat, length);
                        break;
                    case (int)IdolAnimations.BigCall:
                        BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat,             delegate { Arisa.GetComponent<Animator>().Play("IdolBigCall0" + GetPerformanceSuffix(), -1, 0); }),
                        new BeatAction.Action(beat + length,    delegate { Arisa.GetComponent<Animator>().Play("IdolBigCall1" + GetPerformanceSuffix(), -1, 0); }),
                    });
                        break;
                    case (int)IdolAnimations.Squat:
                        BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat,             delegate { Arisa.GetComponent<Animator>().Play("IdolSquat0" + GetPerformanceSuffix(), -1, 0); }),
                        new BeatAction.Action(beat + length,    delegate { Arisa.GetComponent<Animator>().Play("IdolSquat1" + GetPerformanceSuffix(), -1, 0); }),
                    });
                        break;
                    case (int)IdolAnimations.Wink:
                        BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat,             delegate { Arisa.GetComponent<Animator>().Play("IdolWink0" + GetPerformanceSuffix(), -1, 0); }),
                        new BeatAction.Action(beat + length,    delegate { Arisa.GetComponent<Animator>().Play("IdolWink1" + GetPerformanceSuffix(), -1, 0); }),
                    });
                        break;
                    case (int)IdolAnimations.Dab:
                        idolAnimator.Play("IdolDab" + GetPerformanceSuffix(), -1, 0);
                        SoundByte.PlayOneShotGame("fanClub/arisa_dab");
                        break;
                    default: break;
                }
            }
        }

        public void PlayAnimStage(double beat, int type)
        {
            switch (type)
            {
                case (int)StageAnimations.Reset:
                    StageAnimator.Play("Bg", -1, 0);
                    ToSpot();
                    break;
                case (int)StageAnimations.Flash:
                    StageAnimator.Play("Bg_Light", -1, 0);
                    ToSpot();
                    break;
                case (int)StageAnimations.Spot:
                    StageAnimator.Play("Bg_Spot", -1, 0);
                    ToSpot(false);
                    break;
            }
        }

        public void ToSpot(bool unspot = true)
        {
            Arisa.GetComponent<NtrIdolAri>().ToSpot(unspot);
            Blue.ToSpot(unspot);
            Orange.ToSpot(unspot);
            if (unspot)
                spectatorMat.SetColor("_Color", new Color(1, 1, 1, 1));
            else
                spectatorMat.SetColor("_Color", new Color(117 / 255f, 177 / 255f, 209 / 255f, 1));
        }

        private void DoIdolJump(double beat, float length = 3f)
        {
            DisableBop(beat, length);
            DisableResponse(beat, length);
            idolJumpStartTime = beat;

            //play anim
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat,                     delegate { Arisa.GetComponent<Animator>().Play("IdolJump" + GetPerformanceSuffix(), -1, 0); }),
                new BeatAction.Action(beat + 1f,                delegate { Arisa.GetComponent<Animator>().Play("IdolLand" + GetPerformanceSuffix(), -1, 0); }),
            });
        }

        private void DoIdolClaps()
        {
            if (!responseToggle)
            {
                if (!(Conductor.instance.songPositionInBeatsAsDouble >= noResponse.startBeat && Conductor.instance.songPositionInBeatsAsDouble < noResponse.startBeat + noResponse.length))
                {
                    idolAnimator.Play("IdolCrap" + GetPerformanceSuffix(), -1, 0);
                    Blue.PlayAnimState("Crap");
                    Orange.PlayAnimState("Crap");
                }
            }
        }

        private void DoIdolPeace(bool sync = true)
        {
            if (!(Conductor.instance.songPositionInBeatsAsDouble >= noCall.startBeat && Conductor.instance.songPositionInBeatsAsDouble < noCall.startBeat + noCall.length))
            {
                if (sync)
                    idolAnimator.Play("IdolPeace" + GetPerformanceSuffix(), -1, 0);
                else
                    idolAnimator.Play("IdolPeaceNoSync" + GetPerformanceSuffix(), -1, 0);
                Blue.PlayAnimState("Peace");
                Orange.PlayAnimState("Peace");
            }
        }

        private void DoIdolResponse()
        {
            if (responseToggle)
            {
                if (!(Conductor.instance.songPositionInBeatsAsDouble >= noResponse.startBeat && Conductor.instance.songPositionInBeatsAsDouble < noResponse.startBeat + noResponse.length))
                    idolAnimator.Play("IdolResponse" + GetPerformanceSuffix(), -1, 0);
            }
        }

        private void DoIdolCall(int part = 0, bool big = false)
        {
            if (!(Conductor.instance.songPositionInBeatsAsDouble >= noCall.startBeat && Conductor.instance.songPositionInBeatsAsDouble < noCall.startBeat + noCall.length))
            {
                if (big)
                {
                    idolAnimator.Play("IdolBigCall" + part + GetPerformanceSuffix(), -1, 0);
                }
                else
                {
                    idolAnimator.Play("IdolCall" + part + GetPerformanceSuffix(), -1, 0);
                }
            }
        }

        const float HAIS_LENGTH = 4.5f;
        public void CallHai(double beat, bool noSound = false, bool noResponse = false, int type = 0)
        {
            responseToggle = false;
            DisableBop(beat, 8f);

            Prepare(beat + 3f);
            Prepare(beat + 4f);
            Prepare(beat + 5f);
            Prepare(beat + 6f);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat,         delegate { DoIdolPeace();}),
                new BeatAction.Action(beat + 1f,    delegate { DoIdolPeace();}),
                new BeatAction.Action(beat + 2f,    delegate { DoIdolPeace();}),
                new BeatAction.Action(beat + 2.5f,  delegate { DisableSpecBop(beat + 2.5f, 5f);}),
                new BeatAction.Action(beat + 3f,    delegate { DoIdolPeace(false); PlayPrepare(); }),

                new BeatAction.Action(beat + 4f,    delegate { PlayOneClap(beat + 4f); DoIdolClaps();}),
                new BeatAction.Action(beat + 5f,    delegate { PlayOneClap(beat + 5f); DoIdolClaps();}),
                new BeatAction.Action(beat + 6f,    delegate { PlayOneClap(beat + 6f); DoIdolClaps();}),
                new BeatAction.Action(beat + 7f,    delegate { PlayOneClap(beat + 7f); DoIdolClaps();}),
            });
        }

        public static void WarnHai(double beat, bool noSound = false, int type = 0)
        {
            wantHais = beat;
        }

        public static void HaiSound(double beat, bool noSound = false, bool noResponse = false, int type = 0)
        {
            if (!noResponse) PlaySoundSequence("fanClub", "crowd_hai", beat + 4f);
            if (noSound) return;
            PlaySoundSequence("fanClub", "arisa_hai", beat);
        }


        public void ContinueHais(double beat, int type = 0)
        {
            CallHai(beat, true, true, type);
        }

        const float CALL_LENGTH = 2.5f;
        public void CallKamone(double beat, bool noSound = false, bool noResponse = false, int type = 0, int responseType = (int)KamoneResponseType.Through, bool alt = false)
        {
            bool doJump = (responseType == (int)KamoneResponseType.Jump || responseType == (int)KamoneResponseType.JumpFast);
            bool isBig = (responseType == (int)KamoneResponseType.ThroughFast || responseType == (int)KamoneResponseType.JumpFast);
            DisableResponse(beat, 2f);

            responseToggle = true;
            DisableBop(beat, (doJump) ? 6.25f : 5.25f);
            DisableSpecBop(beat + 0.5f, 6f);

            Prepare(beat + 1f, 3);
            Prepare(beat + 2.5f);
            Prepare(beat + 3f, 2);
            Prepare(beat + 4f, 1);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat,                         delegate { DoIdolCall(0, isBig); Blue.PlayAnimState("Beat"); Orange.PlayAnimState("Beat"); }),
                new BeatAction.Action(beat + (isBig ? 1f : 0.75f),  delegate { DoIdolCall(1, isBig); }),
                new BeatAction.Action(beat + 1f,    delegate { PlayPrepare(); Blue.PlayAnimState("Beat"); Orange.PlayAnimState("Beat");  }),

                new BeatAction.Action(beat + 2f,    delegate { PlayLongClap(beat + 2f); DoIdolResponse(); Blue.PlayAnimState("Beat"); Orange.PlayAnimState("Beat");  }),
                new BeatAction.Action(beat + 3f,    delegate { DoIdolResponse(); Blue.PlayAnimState("Beat"); Orange.PlayAnimState("Beat");  }),
                new BeatAction.Action(beat + 3.5f,  delegate { PlayOneClap(beat + 3.5f); }),
                new BeatAction.Action(beat + 4f,    delegate { PlayChargeClap(beat + 4f); DoIdolResponse(); Blue.PlayAnimState("Beat"); Orange.PlayAnimState("Beat");  }),
                new BeatAction.Action(beat + 5f,    delegate { PlayJump(beat + 5f);
                    if (doJump)
                    {
                        DoIdolJump(beat + 5f);
                        Blue.DoIdolJump(beat + 5f);
                        Orange.DoIdolJump(beat + 5f);
                    }
                    else
                    {
                        DoIdolResponse();
                        Blue.PlayAnimState("Beat"); Orange.PlayAnimState("Beat");
                    }
                }),
            });
        }

        public static void WarnKamone(double beat, bool noSound = false, int type = 0, int responseType = (int)KamoneResponseType.Through, bool alt = false)
        {
            wantKamone = beat;
            wantKamoneType = responseType;
            wantKamoneAlt = alt;
        }

        public static void KamoneSound(double beat, bool noSound = false, bool noResponse = false, int type = 0, int responseType = (int)KamoneResponseType.Through, bool alt = false)
        {
            if (!noResponse) PlaySoundSequence("fanClub", alt ? "crowd_iina" : "crowd_kamone", beat + 2f);
            if (noSound) return;
            if (responseType == (int)KamoneResponseType.ThroughFast || responseType == (int)KamoneResponseType.JumpFast)
            {
                PlaySoundSequence("fanClub", alt ? "arisa_iina_fast" : "arisa_kamone_fast", beat);
            }
            else
            {
                PlaySoundSequence("fanClub", alt ? "arisa_iina" : "arisa_kamone", beat);
            }
        }

        public void ContinueKamone(double beat, int type = 0, int responseType = (int)KamoneResponseType.Through, bool alt = false)
        {
            CallKamone(beat, true, true, type, responseType, alt);
        }

        const float BIGCALL_LENGTH = 2.75f;
        public void CallBigReady(double beat, bool noSound = false)
        {
            Prepare(beat + 1.5f);
            Prepare(beat + 2f);

            DisableSpecBop(beat, 3.75f);

            PlayAnimationAll("FanBigReady", onlyOverrideBop: true);
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.5f,   delegate { PlayAnimationAll("FanBigReady", onlyOverrideBop: true); }),
                new BeatAction.Action(beat + 2f,    delegate { PlayAnimationAll("FanBigReady", onlyOverrideBop: true); }),
                new BeatAction.Action(beat + 2.5f,  delegate { PlayOneClap(beat + 2.5f);}),
                new BeatAction.Action(beat + 3f,    delegate { PlayOneClap(beat + 3f);}),
            });
        }

        public static void WarnBigReady(double beat, bool noSound = false)
        {
            wantBigReady = beat;
        }

        public static void BigReadySound(double beat, bool noSound = false)
        {
            if (noSound) return;
            PlaySoundSequence("fanClub", "crowd_big_ready", beat);
        }

        public void ContinueBigReady(double beat)
        {
            CallBigReady(beat, true);
        }

        public void Prepare(double beat, int type = 0)
        {
            Player.AddHit(beat, type);
        }

        private void PlayAnimationAll(string anim, bool noPlayer = false, bool doForced = false, bool onlyOverrideBop = false)
        {
            for (int i = 0; i < Spectators.Count; i++)
            {
                if (i == 3 && noPlayer)
                    continue;

                if (!Spectators[i].GetComponent<Animator>().IsAnimationNotPlaying() && !doForced)
                {
                    if (onlyOverrideBop)
                    {
                        string clipName = Spectators[i].GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name;
                        if (clipName == "FanBeat" || clipName == "NoPose")
                            Spectators[i].GetComponent<Animator>().Play(anim, -1, 0);
                    }
                    continue;
                }
                Spectators[i].GetComponent<Animator>().Play(anim, -1, 0);
            }
        }

        private void BopAll(bool noPlayer = false, bool doForced = false)
        {
            for (int i = 0; i < Spectators.Count; i++)
            {
                if (i == 3 && noPlayer)
                    continue;

                string clipName = Spectators[i].GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name;
                if (!Spectators[i].GetComponent<Animator>().IsAnimationNotPlaying() && !doForced)
                {
                    if (clipName == "FanBeat" || clipName == "NoPose")
                        Spectators[i].GetComponent<NtrIdolFan>().Bop();
                    continue;
                }
                Spectators[i].GetComponent<NtrIdolFan>().Bop();
            }
        }

        private void PlayPrepare()
        {
            for (int i = 0; i < Spectators.Count; i++)
            {
                if (Spectators[i].GetComponent<NtrIdolFan>().IsJumping())
                    continue;
                Spectators[i].GetComponent<Animator>().Play("FanPrepare", -1, 0);
            }
        }

        private void PlayOneClap(double beat, int who = -1)
        {
            if (who != -1)
            {
                if (who == 3)
                {
                    if (GameManager.instance.autoplay)
                    {
                        Player.ClapStart(true, false, 0.1f);
                    }
                    return;
                }
                // Jukebox.PlayOneShotGame("fanClub/play_clap", volume: 0.08f);
                SoundByte.PlayOneShotGame("fanClub/crap_impact", pitch: UnityEngine.Random.Range(0.95f, 1.05f), volume: 0.1f);
                BeatAction.New(Spectators[who].GetComponent<NtrIdolFan>(), new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { Spectators[who].GetComponent<Animator>().Play("FanClap", -1, 0); }),
                    new BeatAction.Action(beat + 0.1f, delegate { Spectators[who].GetComponent<Animator>().Play("FanFree", -1, 0); }),

                });
            }
            else
            {
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { PlayAnimationAll("FanClap", true, true);}),
                    new BeatAction.Action(beat + 0.1f, delegate { PlayAnimationAll("FanFree", true, true);}),
                });
            }
        }

        private void PlayLongClap(double beat)
        {
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { PlayAnimationAll("FanClap", true, true);}),
                new BeatAction.Action(beat + 1f, delegate { PlayAnimationAll("FanFree", true, true);}),
            });
        }

        private void PlayChargeClap(double beat)
        {
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { PlayAnimationAll("FanClap", true, true);}),
                new BeatAction.Action(beat + 0.1f, delegate { PlayAnimationAll("FanClapCharge", true, true);}),
            });
        }

        private void StartJump(int idx, double beat)
        {
            Spectators[idx].GetComponent<NtrIdolFan>().jumpStartTime = beat;
            BeatAction.New(Spectators[idx].GetComponent<NtrIdolFan>(), new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { Spectators[idx].GetComponent<Animator>().Play("FanJump", -1, 0);}),
                new BeatAction.Action(beat + 1f, delegate { Spectators[idx].GetComponent<Animator>().Play("FanPrepare", -1, 0);}),
            });
        }

        private void PlayJump(double beat)
        {
            for (int i = 0; i < Spectators.Count; i++)
            {
                if (i == 3)
                    continue;

                StartJump(i, beat);
            }
        }

        public void AngerOnMiss()
        {
            for (int i = 0; i <= 5; i++)
            {
                if (i == 3)
                    continue;
                Spectators[i].GetComponent<NtrIdolFan>().MakeAngry(i > 3);
            }
        }

        public void DancerTravel(double beat, float length, bool exit, bool instant)
        {
            if (instant)
            {
                Blue.FinishEntrance(exit);
                Orange.FinishEntrance(exit);
            }
            else
            {
                Blue.StartEntrance(beat, length, exit);
                Orange.StartEntrance(beat, length, exit);
            }
        }

        public void FinalCheer(double beat)
        {
            if (noJudgement) return;
            noJudgement = true;
            noJudgementInput = false;

            // recreation of sub61
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { StartClapLoop(beat, 1);}),

                new BeatAction.Action(beat + (2f/3f), delegate { StartClapLoop(beat + (2f/3f), 0);}),
                new BeatAction.Action(beat + (2f/3f), delegate { StartClapLoop(beat + (2f/3f), 3);}),

                new BeatAction.Action(beat + (2f/3f) + 0.25f, delegate { StartClapLoop(beat + (2f/3f) + 0.25f, 6);}),
                new BeatAction.Action(beat + (2f/3f) + 0.25f, delegate { StartClapLoop(beat + (2f/3f) + 0.25f, 8);}),

                new BeatAction.Action(beat + (2f/3f) + 0.5f, delegate { StartClapLoop(beat + (2f/3f) + 0.5f, 7);}),
                new BeatAction.Action(beat + (2f/3f) + 0.5f, delegate { StartClapLoop(beat + (2f/3f) + 0.5f, 4);}),

                new BeatAction.Action(beat + 1.5f, delegate { StartClapLoop(beat + 1.5f, 2);}),
                new BeatAction.Action(beat + 1.5f, delegate { StartClapLoop(beat + 1.5f, 11);}),

                new BeatAction.Action(beat + 1.5f + (1f/3f), delegate { StartClapLoop(beat + 1.5f + (1f/3f), 5);}),
                new BeatAction.Action(beat + 1.5f + (1f/3f), delegate { StartClapLoop(beat + 1.5f + (1f/3f), 10);}),

                new BeatAction.Action(beat + 2f + (1f/3f), delegate { StartClapLoop(beat + 2f + (1f/3f), 9);}),

                // 0x113
                new BeatAction.Action(beat + 6f , delegate { CheckApplause();}),
            });

            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("fanClub/play_jump", beat,                     pitch: 1f, volume: 0.6f),
                new MultiSound.Sound("fanClub/play_jump", beat + (2f/3f),           pitch: 0.98f, volume: 0.5f),
                new MultiSound.Sound("fanClub/play_jump", beat + (2f/3f) + 0.25f,   pitch: UnityEngine.Random.Range(0.9f, 1.05f), volume: 0.6f),
                new MultiSound.Sound("fanClub/play_jump", beat + (2f/3f) + 0.5f,    pitch: UnityEngine.Random.Range(0.9f, 1.05f), volume: 0.6f),
                new MultiSound.Sound("fanClub/play_jump", beat + 1.5f,              pitch: UnityEngine.Random.Range(0.9f, 1.05f), volume: 0.6f),
                new MultiSound.Sound("fanClub/play_jump", beat + 1.5f + (1f/3f),    pitch: UnityEngine.Random.Range(0.9f, 1.05f), volume: 0.6f),
                new MultiSound.Sound("fanClub/play_jump", beat + 2f + (1f/3f),      pitch: UnityEngine.Random.Range(0.9f, 1.05f), volume: 0.6f),
            });
        }

        void StartClapLoop(double beat, int who)
        {
            BeatAction.New(Spectators[who].GetComponent<NtrIdolFan>(), new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { PlayOneClap(beat, who); }),
                new BeatAction.Action(beat + 0.5f, delegate { StartClapLoop(beat + 0.5f, who); }),
            });
        }

        void CheckApplause()
        {
            if (!noJudgementInput)
            {
                AngerOnMiss();
                // fuck you
                FanClub.instance.ScoreMiss(69);
            }
        }
    }
}
