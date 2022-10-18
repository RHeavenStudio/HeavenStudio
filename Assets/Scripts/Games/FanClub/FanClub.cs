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
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("fanClub", "Fan Club", "FDFD00", false, false, new List<GameAction>()
                {
                    new GameAction("bop", "Bop")
                    {
                        function = delegate { var e = eventCaller.currentEntity; FanClub.instance.Bop(e.beat, e.length, e["type"]); }, 
                        defaultLength = 0.5f, 
                        resizable = true, 
                        parameters = new List<Param>()
                        {
                            new Param("type", FanClub.IdolBopType.Both, "Bop target", "Who to make bop"),
                        }
                    },
                    new GameAction("yeah, yeah, yeah", "Yeah, Yeah, Yeah!")
                    {
                        function = delegate { var e = eventCaller.currentEntity; FanClub.instance.CallHai(e.beat, e["toggle"]); }, 
                        defaultLength = 8,
                        parameters = new List<Param>()
                        {
                            new Param("toggle", false, "Disable call", "Disable the idol's call")
                        },
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; FanClub.WarnHai(e.beat, e["toggle"]);}
                    },
                    new GameAction("I suppose", "I Suppose!")
                    {
                        function = delegate { var e = eventCaller.currentEntity; FanClub.instance.CallKamone(e.beat, e["toggle"], 0, e["type"]); }, 
                        defaultLength = 6, 
                        parameters = new List<Param>()
                        {
                            new Param("type", FanClub.KamoneResponseType.Through, "Response type", "Type of response to use"),
                            new Param("toggle", false, "Disable call", "Disable the idol's call")
                        },
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; FanClub.WarnKamone(e.beat, e["toggle"], 0, e["type"]);}
                    },
                    new GameAction("double clap", "Double Clap")
                    {
                        function = delegate { var e = eventCaller.currentEntity; FanClub.instance.CallBigReady(e.beat, e["toggle"]); }, 
                        defaultLength = 4,
                        parameters = new List<Param>()
                        {
                            new Param("toggle", false, "Disable call", "Disable the call")
                        },
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; FanClub.WarnBigReady(e.beat, e["toggle"]); }
                    },
                    new GameAction("play idol animation", "Idol Coreography")
                    {
                        function = delegate { var e = eventCaller.currentEntity; FanClub.instance.PlayAnim(e.beat, e.length, e["type"]); },
                        resizable = true, 
                        parameters = new List<Param>()
                        {
                            new Param("type", FanClub.IdolAnimations.Bop, "Animation", "Animation to play")
                        }
                    },
                    new GameAction("play stage animation", "Stage Coreography")
                    {
                        function = delegate { var e = eventCaller.currentEntity; FanClub.instance.PlayAnimStage(e.beat, e["type"]); }, 
                        resizable = true, 
                        parameters = new List<Param>()
                        {
                            new Param("type", FanClub.StageAnimations.Flash, "Animation", "Animation to play")
                        }
                    },
                    new GameAction("set performance type", "Coreography Type")
                    {

                        function = delegate { var e = eventCaller.currentEntity; FanClub.SetPerformanceType(e["type"]);},
                        defaultLength = 0.5f,
                        parameters = new List<Param>()
                        {
                            new Param("type", FanClub.IdolPerformanceType.Normal, "Performance Type", "Set of animations for the idol to use")
                        },
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; FanClub.SetPerformanceType(e["type"]); }
                    },
                },
                new List<string>() {"ntr", "normal"},
                "ntridol", "jp",
                new List<string>() {"jp"}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_FanClub;
    
    public class FanClub : Minigame
    {
        public enum IdolBopType {
            Both,
            Idol,
            Spectators
        }
        public enum IdolAnimations {
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
        public enum KamoneResponseType {
            Through,
            Jump,
            ThroughFast,
            JumpFast,
        }
        public enum StageAnimations {
            Reset,
            Flash,
            Spot
        }
        public enum IdolPerformanceType {
            Normal,
            Arrange,
            // Tour(this one is fan made so ?)
        }

        // userdata here
        [Header("Animators")]
        //stage
        public Animator StageAnimator;

        [Header("Objects")]
        public GameObject Arisa;
        public GameObject ArisaRootMotion;
        public GameObject ArisaShadow;
        public GameObject spectator;
        public GameObject spectatorAnchor;

        [Header("References")]
        public Material spectatorMat;

        // end userdata

        //arisa's animation controller
        private Animator idolAnimator;
        //spectators
        private List<GameObject> Spectators;
        public NtrIdolFan Player;

        //bop-type animations
        public GameEvent bop = new GameEvent();
        public GameEvent specBop = new GameEvent();
        public GameEvent noBop = new GameEvent();
        public GameEvent noResponse = new GameEvent();
        public GameEvent noCall = new GameEvent();
        public GameEvent noSpecBop = new GameEvent();

        private static int performanceType = (int) IdolPerformanceType.Normal;
        private bool responseToggle = false;
        private static float wantHais = Single.MinValue;
        private static float wantKamone = Single.MinValue;
        private static int wantKamoneType = (int) KamoneResponseType.Through;
        private static float wantBigReady = Single.MinValue;
        public float idolJumpStartTime = Single.MinValue;
        private bool hasJumped = false;

        //game scene
        public static FanClub instance;

        const int FAN_COUNT = 12;
        const float RADIUS = 1.5f;
        private void Awake()
        {
            instance = this;

            Spectators = new List<GameObject>();
            idolAnimator = Arisa.GetComponent<Animator>();

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

            if (performanceType != (int) IdolPerformanceType.Normal)
            {
                idolAnimator.Play("NoPose" + GetPerformanceSuffix(), -1, 0);
            }

            ToSpot();
        }

        public static string GetPerformanceSuffix()
        {
            switch (performanceType)
            {
                case (int) IdolPerformanceType.Arrange:
                    return "Arrange";
                default:
                    return "";
            }
        }
        
        public static void SetPerformanceType(int type = (int) IdolPerformanceType.Normal)
        {
            performanceType = type;
            if (GameManager.instance.currentGame == "fanClub")
            {
                FanClub.instance.idolAnimator.Play("NoPose" + GetPerformanceSuffix(), -1, 0);
            }
        }

        public override void OnGameSwitch(float beat)
        {
            if (wantHais != Single.MinValue)
            {
                ContinueHais(wantHais);
                wantHais = Single.MinValue;
            }
            if (wantKamone != Single.MinValue)
            {
                ContinueKamone(wantKamone, 0, wantKamoneType);
                wantKamone = Single.MinValue;
            }
            if (wantBigReady != Single.MinValue)
            {
                ContinueBigReady(wantBigReady);
                wantBigReady = Single.MinValue;
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                if (cond.songPositionInBeats >= bop.startBeat && cond.songPositionInBeats < bop.startBeat + bop.length)
                {
                    if (!(cond.songPositionInBeats >= noBop.startBeat && cond.songPositionInBeats < noBop.startBeat + noBop.length))
                        idolAnimator.Play("IdolBeat" + GetPerformanceSuffix(), 0, 0);
                }
            }

            if (cond.ReportBeat(ref specBop.lastReportedBeat, specBop.startBeat % 1))
            {
                if (cond.songPositionInBeats >= specBop.startBeat && cond.songPositionInBeats < specBop.startBeat + specBop.length)
                {
                    if (!(cond.songPositionInBeats >= noSpecBop.startBeat && cond.songPositionInBeats < noSpecBop.startBeat + noSpecBop.length))
                        BopAll();
                }
            }
            
            //idol jumping physics
            float jumpPos = cond.GetPositionFromBeat(idolJumpStartTime, 1f);
            float IDOL_SHADOW_SCALE = 1.18f;
            if (cond.songPositionInBeats >= idolJumpStartTime && cond.songPositionInBeats < idolJumpStartTime + 1f)
            {
                hasJumped = true;
                float yMul = jumpPos * 2f - 1f;
                float yWeight = -(yMul*yMul) + 1f;
                //TODO: idol start position
                ArisaRootMotion.transform.localPosition = new Vector3(0, 2f * yWeight + 0.25f);
                ArisaShadow.transform.localScale = new Vector3((1f-yWeight*0.8f) * IDOL_SHADOW_SCALE, (1f-yWeight*0.8f) * IDOL_SHADOW_SCALE, 1f);
            }
            else
            {
                if (hasJumped)
                {
                    //DisableBop(cond.songPositionInBeats, 1.5f);
                    //TODO: landing anim
                }
                idolJumpStartTime = Single.MinValue;
                //TODO: idol start position
                ArisaRootMotion.transform.localPosition = new Vector3(0, 0);
                ArisaShadow.transform.localScale = new Vector3(IDOL_SHADOW_SCALE, IDOL_SHADOW_SCALE, 1f);
            }
        }

        public void Bop(float beat, float length, int target = (int) IdolBopType.Both)
        {
            if (target == (int) IdolBopType.Both || target == (int) IdolBopType.Idol)
            {
                bop.length = length;
                bop.startBeat = beat;
            }

            if (target == (int) IdolBopType.Both || target == (int) IdolBopType.Spectators)
                SpecBop(beat, length);
        }

        public void SpecBop(float beat, float length)
        {
            specBop.length = length;
            specBop.startBeat = beat;
        }

        private void DisableBop(float beat, float length)
        {
            noBop.length = length;
            noBop.startBeat = beat;
        }

        private void DisableResponse(float beat, float length)
        {
            noResponse.length = length;
            noResponse.startBeat = beat;
        }

        private void DisableCall(float beat, float length)
        {
            noCall.length = length;
            noCall.startBeat = beat;
        }

        private void DisableSpecBop(float beat, float length)
        {
            float bt = Conductor.instance.songPositionInBeats;
            if (bt >= noSpecBop.startBeat && bt < noSpecBop.startBeat + noSpecBop.length)
            {
                float thisStToNextSt = beat - noSpecBop.startBeat;
                float newLen = thisStToNextSt + length;
                if (newLen > noSpecBop.length)
                    noSpecBop.length = thisStToNextSt + length;
            }
            else
            {
                noSpecBop.length = length;
                noSpecBop.startBeat = beat;
            }
        }

        public void PlayAnim(float beat, float length, int type)
        {
            idolJumpStartTime = Single.MinValue;
            DisableResponse(beat, length + 0.5f);
            DisableBop(beat, length + 0.5f);
            DisableCall(beat, length + 0.5f);

            switch (type)
            {
                case (int) IdolAnimations.Bop:
                    idolAnimator.Play("IdolBeat" + GetPerformanceSuffix(), -1, 0);
                    break;
                case (int) IdolAnimations.PeaceVocal:
                    idolAnimator.Play("IdolPeace" + GetPerformanceSuffix(), -1, 0);
                    break;
                case (int) IdolAnimations.Peace:
                    idolAnimator.Play("IdolPeaceNoSync" + GetPerformanceSuffix(), -1, 0);
                    break;
                case (int) IdolAnimations.Clap:
                    idolAnimator.Play("IdolCrap" + GetPerformanceSuffix(), -1, 0);
                    break;
                case (int) IdolAnimations.Call:
                    BeatAction.New(Arisa, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat,             delegate { Arisa.GetComponent<Animator>().Play("IdolCall0" + GetPerformanceSuffix(), -1, 0); }),
                        new BeatAction.Action(beat + 0.75f,     delegate { Arisa.GetComponent<Animator>().Play("IdolCall1" + GetPerformanceSuffix(), -1, 0); }),
                    });
                    break;
                case (int) IdolAnimations.Response:
                    idolAnimator.Play("IdolResponse" + GetPerformanceSuffix(), -1, 0);
                    break;
                case (int) IdolAnimations.Jump:
                    DoIdolJump(beat, length);
                    break;
                case (int) IdolAnimations.BigCall:
                    BeatAction.New(Arisa, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat,             delegate { Arisa.GetComponent<Animator>().Play("IdolBigCall0" + GetPerformanceSuffix(), -1, 0); }),
                        new BeatAction.Action(beat + length,    delegate { Arisa.GetComponent<Animator>().Play("IdolBigCall1" + GetPerformanceSuffix(), -1, 0); }),
                    });
                    break;
                case (int) IdolAnimations.Squat:
                    BeatAction.New(Arisa, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat,             delegate { Arisa.GetComponent<Animator>().Play("IdolSquat0" + GetPerformanceSuffix(), -1, 0); }),
                        new BeatAction.Action(beat + length,    delegate { Arisa.GetComponent<Animator>().Play("IdolSquat1" + GetPerformanceSuffix(), -1, 0); }),
                    });
                    break;
                case (int) IdolAnimations.Wink:
                    BeatAction.New(Arisa, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat,             delegate { Arisa.GetComponent<Animator>().Play("IdolWink0" + GetPerformanceSuffix(), -1, 0); }),
                        new BeatAction.Action(beat + length,    delegate { Arisa.GetComponent<Animator>().Play("IdolWink1" + GetPerformanceSuffix(), -1, 0); }),
                    });
                    break;
                case (int) IdolAnimations.Dab:
                    idolAnimator.Play("IdolDab" + GetPerformanceSuffix(), -1, 0);
                    Jukebox.PlayOneShotGame("fanClub/arisa_dab");
                    break;
                default: break;
            }
        }

        public void PlayAnimStage(float beat, int type)
        {
            switch (type)
            {
                case (int) StageAnimations.Reset:
                    StageAnimator.Play("Bg", -1, 0);
                    ToSpot();
                    break;
                case (int) StageAnimations.Flash:
                    StageAnimator.Play("Bg_Light", -1, 0);
                    ToSpot();
                    break;
                case (int) StageAnimations.Spot:
                    StageAnimator.Play("Bg_Spot", -1, 0);
                    ToSpot(false);
                    break;
            }
        }

        public void ToSpot(bool unspot = true)
        {
            Arisa.GetComponent<NtrIdolAri>().ToSpot(unspot);
            if (unspot)
                spectatorMat.SetColor("_Color", new Color(1, 1, 1, 1));
            else
                spectatorMat.SetColor("_Color", new Color(117/255f, 177/255f, 209/255f, 1));
        }

        private void DoIdolJump(float beat, float length = 3f)
        {
            DisableBop(beat, length);
            DisableResponse(beat, length);
            idolJumpStartTime = beat;

            //play anim
            BeatAction.New(Arisa, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat,                     delegate { Arisa.GetComponent<Animator>().Play("IdolJump" + GetPerformanceSuffix(), -1, 0); }),
                new BeatAction.Action(beat + 1f,                delegate { Arisa.GetComponent<Animator>().Play("IdolLand" + GetPerformanceSuffix(), -1, 0); }),
            });
        }

        private void DoIdolClaps()
        {
            if (!responseToggle)
            {
                if (!(Conductor.instance.songPositionInBeats >= noResponse.startBeat && Conductor.instance.songPositionInBeats < noResponse.startBeat + noResponse.length))
                {
                    idolAnimator.Play("IdolCrap" + GetPerformanceSuffix(), -1, 0);
                }
            }
        }

        private void DoIdolPeace(bool sync = true)
        {
            if (!(Conductor.instance.songPositionInBeats >= noCall.startBeat && Conductor.instance.songPositionInBeats < noCall.startBeat + noCall.length))
            {
                if (sync)
                    idolAnimator.Play("IdolPeace" + GetPerformanceSuffix(), -1, 0);
                else
                    idolAnimator.Play("IdolPeaceNoSync" + GetPerformanceSuffix(), -1, 0);
            }
        }

        private void DoIdolResponse()
        {
            if (responseToggle)
            {
                if (!(Conductor.instance.songPositionInBeats >= noResponse.startBeat && Conductor.instance.songPositionInBeats < noResponse.startBeat + noResponse.length))
                    idolAnimator.Play("IdolResponse" + GetPerformanceSuffix(), -1, 0);
            }
        }

        private void DoIdolCall(int part = 0, bool big = false)
        {
            if (!(Conductor.instance.songPositionInBeats >= noCall.startBeat && Conductor.instance.songPositionInBeats < noCall.startBeat + noCall.length))
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
        public void CallHai(float beat, bool noSound = false, int type = 0)
        {
            if (!noSound)
                MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound("fanClub/arisa_hai_1_jp", beat), 
                    new MultiSound.Sound("fanClub/arisa_hai_2_jp", beat + 1f),
                    new MultiSound.Sound("fanClub/arisa_hai_3_jp", beat + 2f),
                });

            responseToggle = false;
            DisableBop(beat, 8f);

            Prepare(beat + 3f); 
            Prepare(beat + 4f); 
            Prepare(beat + 5f); 
            Prepare(beat + 6f); 

            BeatAction.New(Arisa, new List<BeatAction.Action>()
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

            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("fanClub/crowd_hai_jp", beat + 4f), 
                new MultiSound.Sound("fanClub/crowd_hai_jp", beat + 5f),
                new MultiSound.Sound("fanClub/crowd_hai_jp", beat + 6f),
                new MultiSound.Sound("fanClub/crowd_hai_jp", beat + 7f),
            });
        }

        public static void WarnHai(float beat, bool noSound = false, int type = 0)
        {
            wantHais = beat;
            if (noSound) return;
            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("fanClub/arisa_hai_1_jp", beat), 
                new MultiSound.Sound("fanClub/arisa_hai_2_jp", beat + 1f),
                new MultiSound.Sound("fanClub/arisa_hai_3_jp", beat + 2f),
            }, forcePlay:true);
        }

        public void ContinueHais(float beat, int type = 0)
        {
            CallHai(beat, true, type);
        }

        const float CALL_LENGTH = 2.5f;
        public void CallKamone(float beat, bool noSound = false, int type = 0, int responseType = (int) KamoneResponseType.Through)
        {
            bool doJump = (responseType == (int) KamoneResponseType.Jump || responseType == (int) KamoneResponseType.JumpFast);
            bool isBig = (responseType == (int) KamoneResponseType.ThroughFast || responseType == (int) KamoneResponseType.JumpFast);
            DisableResponse(beat, 2f);
            if (isBig)
            {
                if (!noSound)
                {
                    MultiSound.Play(new MultiSound.Sound[] { 
                        new MultiSound.Sound("fanClub/arisa_ka_fast_jp", beat), 
                        new MultiSound.Sound("fanClub/arisa_mo_fast_jp", beat + 0.25f),
                        new MultiSound.Sound("fanClub/arisa_ne_fast_jp", beat + 0.5f),
                    });
                }
            }
            else
            {
                if (!noSound)
                {
                    MultiSound.Play(new MultiSound.Sound[] { 
                        new MultiSound.Sound("fanClub/arisa_ka_jp", beat), 
                        new MultiSound.Sound("fanClub/arisa_mo_jp", beat + 0.5f, offset: 0.07407407f),
                        new MultiSound.Sound("fanClub/arisa_ne_jp", beat + 1f, offset: 0.07407407f),
                    });
                }
            }

            responseToggle = true;
            DisableBop(beat, (doJump) ? 6.25f : 5.25f);
            DisableSpecBop(beat + 0.5f, 6f);

            Prepare(beat + 1f, 3);
            Prepare(beat + 2.5f);
            Prepare(beat + 3f, 2);
            Prepare(beat + 4f, 1);

            BeatAction.New(Arisa, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat,                         delegate { DoIdolCall(0, isBig); }),
                new BeatAction.Action(beat + (isBig ? 1f : 0.75f),  delegate { DoIdolCall(1, isBig); }),
                new BeatAction.Action(beat + 1f,    delegate { PlayPrepare(); }),

                new BeatAction.Action(beat + 2f,    delegate { PlayLongClap(beat + 2f); DoIdolResponse(); }),
                new BeatAction.Action(beat + 3f,    delegate { DoIdolResponse(); }),
                new BeatAction.Action(beat + 3.5f,  delegate { PlayOneClap(beat + 3.5f); }),
                new BeatAction.Action(beat + 4f,    delegate { PlayChargeClap(beat + 4f); DoIdolResponse(); }),
                new BeatAction.Action(beat + 5f,    delegate { PlayJump(beat + 5f);
                    if (doJump) 
                    {
                        DoIdolJump(beat + 5f);
                    }
                    else
                    {
                        DoIdolResponse();
                    }
                }),
            });

            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("fanClub/crowd_ka_jp", beat + 2f), 
                new MultiSound.Sound("fanClub/crowd_mo_jp", beat + 3.5f),
                new MultiSound.Sound("fanClub/crowd_ne_jp", beat + 4f),
                new MultiSound.Sound("fanClub/crowd_hey_jp", beat + 5f),
            });
        }

        public static void WarnKamone(float beat, bool noSound = false, int type = 0, int responseType = (int) KamoneResponseType.Through)
        {
            wantKamone = beat;
            wantKamoneType = responseType;
            if (noSound) return;
            if (responseType == (int) KamoneResponseType.ThroughFast || responseType == (int) KamoneResponseType.JumpFast)
            {
                MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound("fanClub/arisa_ka_fast_jp", beat), 
                    new MultiSound.Sound("fanClub/arisa_mo_fast_jp", beat + 0.25f),
                    new MultiSound.Sound("fanClub/arisa_ne_fast_jp", beat + 0.5f),
                }, forcePlay:true);
            }
            else
            {
                MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound("fanClub/arisa_ka_jp", beat), 
                    new MultiSound.Sound("fanClub/arisa_mo_jp", beat + 0.5f, offset: 0.07407407f),
                    new MultiSound.Sound("fanClub/arisa_ne_jp", beat + 1f, offset: 0.07407407f),
                }, forcePlay:true);
            }
        }

        public void ContinueKamone(float beat, int type = 0, int responseType = (int) KamoneResponseType.Through)
        {
            CallKamone(beat, true, type, responseType);
        }

        const float BIGCALL_LENGTH = 2.75f;
        public void CallBigReady(float beat, bool noSound = false)
        {
            Prepare(beat + 1.5f);
            Prepare(beat + 2f);

            if (!noSound)
                Jukebox.PlayOneShotGame("fanClub/crowd_big_ready");
            
            DisableSpecBop(beat, 3.75f);

            PlayAnimationAll("FanBigReady", onlyOverrideBop: true);
            BeatAction.New(this.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.5f,   delegate { PlayAnimationAll("FanBigReady", onlyOverrideBop: true); }),
                new BeatAction.Action(beat + 2f,    delegate { PlayAnimationAll("FanBigReady", onlyOverrideBop: true); }),
                new BeatAction.Action(beat + 2.5f,  delegate { PlayOneClap(beat + 2.5f);}),
                new BeatAction.Action(beat + 3f,    delegate { PlayOneClap(beat + 3f);}),
            });
        }

        public static void WarnBigReady(float beat, bool noSound = false)
        {
            wantBigReady = beat;
            if (noSound) return;
            Jukebox.PlayOneShotGame("fanClub/crowd_big_ready");
        }

        public void ContinueBigReady(float beat)
        {
            CallBigReady(beat, true);
        }

        public void Prepare(float beat, int type = 0)
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

        private void PlayOneClap(float beat)
        {
            BeatAction.New(this.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { PlayAnimationAll("FanClap", true, true);}),
                new BeatAction.Action(beat + 0.1f, delegate { PlayAnimationAll("FanFree", true, true);}),
            });
        }

        private void PlayLongClap(float beat)
        {
            BeatAction.New(this.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { PlayAnimationAll("FanClap", true, true);}),
                new BeatAction.Action(beat + 1f, delegate { PlayAnimationAll("FanFree", true, true);}),
            });
        }

        private void PlayChargeClap(float beat)
        {
            BeatAction.New(this.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { PlayAnimationAll("FanClap", true, true);}),
                new BeatAction.Action(beat + 0.1f, delegate { PlayAnimationAll("FanClapCharge", true, true);}),
            });
        }

        private void StartJump(int idx, float beat)
        {
            Spectators[idx].GetComponent<NtrIdolFan>().jumpStartTime = beat;
            BeatAction.New(Spectators[idx], new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { Spectators[idx].GetComponent<Animator>().Play("FanJump", -1, 0);}),
                new BeatAction.Action(beat + 1f, delegate { Spectators[idx].GetComponent<Animator>().Play("FanPrepare", -1, 0);}),
            });
        }

        private void PlayJump(float beat)
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
    }
}