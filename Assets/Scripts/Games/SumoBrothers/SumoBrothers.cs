// Hello my brothers, it is RaffyTaffy14 here in the code comments with another monologue.
// I still do not know much crap about Unity or C#.
// Despite that, I have managed to cobble together Sumo Brothers in a fairly functional state.
// A lot of the code may be very weird or just useless (especially the older stuff), but it works for now.
// I started making Sumo Brothers for Heaven Studio all the way back in January of 2023 (13 months ago!)
// Sumo Brothers at that time was VERY janky. Heck, I used blurry screenshots from videos as reference guides for everything.
// Progress on Sumo stopped a few weeks into the project, being as functional as how I left Lockstep for rasmus to pick up.
// In June, I finally learned how to cue inputs (I was doing something wrong for the longest time), so now Finishing Poses could be cued.
// Nothing happened until December, when I coded most of the functionality for the Slapping/Stomping/Posing
// switching, which is how it currently works now.

// From the end of January 2024 to the middle of February 2024, I spent dozens of hours meticulously animating
// every animation (there's *74* when you factor out duplicate animations for the Glasses Brother) for the game, from "SumoSlapPrepare" all
// the way to "SumoPoseGBopMiss2".
// No one thought that there would be anyone crazy enough to hecking animate Sumo Brothers.
// People said that it would be animator suicide to even attempt to animate Sumo Brothers.
// Yet, here I am, still alive and still too crazy.

// Sometime during the mass animation process, I managed to get back in contact with the upscaler for this game for a potential
// redo of the Inu Sensei sprites.
// Let's just say that the sprites got a MASSIVE glow up (thx UnbrokenMage)
// Animation for Sumo Brothers still isn't done, as I am yet to animate the 3rd and 4th poses which are (currently) absent.
// Despite the absense for those 20 animations (yes 2 poses takes up 20 animations), I have managed to do a whopping 54 animations
// during my ~2 week grind.

// I hope that whomever may be reading this shall enjoy the wonders of Sumo Brothers and have a great rest of your day. 
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrSumouLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("sumoBrothers", "Sumo Brothers", "EDED15", false, false, new List<GameAction>()
            {
                
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.Bop(e.beat, e.length, e["bopInu"], e["bopSumo"], e["bopInuAuto"], e["bopSumoAuto"]); },
                    parameters = new List<Param>()
                    {
                        new Param("bopInu", true, "Bop Inu", "Whether Inu Sensei should bop."),
                        new Param("bopSumo", true, "Bop Sumo", "Whether the Sumo Brothers should bop."),
                        new Param("bopInuAuto", false, "Bop Inu (Auto)", "Whether Inu Sensei should bop automatically."),
                        new Param("bopSumoAuto", false, "Bop Sumo (Auto)", "Whether the Sumo Brothers should bop automatically."),
                    },
                    defaultLength = 1f,
                    resizable = true
                },

                new GameAction("crouch", "Crouch")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.Crouch(e.beat, e.length, e["inuT"], e["sumoT"]); },
                    parameters = new List<Param>()
                    {
                        new Param("inuT", true, "Inu Sensei", "Whether Inu Sensei should crouch."),
                        new Param("sumoT", true, "Sumo Brothers", "Whether the Sumo Brothers should crouch.")
                    },
                    defaultLength = 1f,
                    resizable = true
                },

                new GameAction("stompSignal", "Stomp Signal")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.StompSignal(e.beat, e["mute"], e["look"]); },
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute", "Disables Inu Sensei's sound cues and animations."),
                        new Param("look", true, "Look Forward", "The Sumo Brothers will look at the camera if transitioning from slapping.")
                    },
                    defaultLength = 4f,
                    priority = 4
                },

                new GameAction("slapSignal", "Slap Signal")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.SlapSignal(e.beat, e["mute"]); },
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute", "Disables Inu Sensei's sound cues and animations.")
                    },
                    defaultLength = 4f,
                    priority = 3
                },

                new GameAction("endPose", "Finishing Pose")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.EndPose(e.beat, e["random"], e["type"], e["bg"], e["confetti"]); },
                    parameters = new List<Param>()
                    {   new Param("random", true, "Random Pose", "Picks a random pose that will play on a successful input. Does not include the finale pose.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "type" })
                        }),
                        new Param("type", new EntityTypes.Integer(1, 2, 1), "Pose", "The pose that the Sumo Brothers will make."),
                        new Param("bg", SumoBrothers.BGType.None, "Background", "The background that appears on a successful input."),
                        new Param("confetti", false, "Confetti (WIP)", "Confetti particles will fly everywhere on a successful input.")
                    },
                    defaultLength = 5f,
                    priority = 2
                },

                new GameAction("look", "Look Forward")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.LookAtCamera(e.beat, e.length); },
                    /*parameters = new List<Param>()
                    {
                        new Param("look", true, "Look at Camera", "Whether the Sumo Brothers will look at the camera while slapping."),
                    },*/
                    defaultLength = 1f,
                    resizable = true
                },

            });
        }
    }
}

namespace HeavenStudio.Games
{
    //    using Scripts_SumoBrothers;
    public class SumoBrothers : Minigame
    {
        [Header("Components")]
        [SerializeField] Animator inuSensei;
        [SerializeField] Animator sumoBrotherP;
        [SerializeField] Animator sumoBrotherG;
        [SerializeField] Animator sumoBrotherGHead;
        [SerializeField] Animator sumoBrotherPHead;
        [SerializeField] Animator impact;
        [SerializeField] Animator dust;


        [Header("Properties")]
        /*static List<queuedSumoInputs> queuedInputs = new List<queuedSumoInputs>();
        public struct queuedSumoInputs
        {
            public float beat;
            public int cue;
            public int poseType;
            public int poseBG;
        }*/

        private bool goBopSumo;
        private bool goBopInu;

        private bool allowBopSumo;
        private bool allowBopInu;

        private bool sumoStompDir;
        private int sumoSlapDir;
        private int sumoPoseType;
        private string sumoPoseTypeCurrent = "1";
        private int sumoPoseTypeNext;

        private bool lookingAtCamera = false;

        private double lastReportedBeat = 0f;

        private bool cueCurrentlyActive; 
        private double cueCurrentlyActiveBeat;
        
        //private var stompInput;

        const int IAAltDownCat = IAMAXCAT;

        public static SumoBrothers instance;

        /* public enum PoseType
        {
            Crouching,
            Crossed,
            Pointing,
            Finale
        }*/

        public enum BGType
        {
            //TheGreatWave = 0,
            //OtaniOniji = 1,
            None = 2
        }

        private enum SumoState
        {
            Idle,
            Slap,
            Stomp,
            Pose
        }
        private SumoState sumoState = SumoState.Idle;
        private SumoState sumoStatePrevious = SumoState.Idle;

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }
        protected static bool IA_TouchAltPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_Alt);
        }

        public static PlayerInput.InputAction InputAction_Alt =
        new("CtrSumouAlt", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
        IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);


        // Start is called before the first frame update
        void Awake()
        {
            goBopInu = true;
            goBopSumo = true;
            allowBopInu = true;
            allowBopSumo = true;
            instance = this;
            cueCurrentlyActive = false;

            sumoStompDir = false;
            sumoSlapDir = 0;

            sumoPoseType = 0;
            sumoPoseTypeNext = 0;

        }

        void OnDestroy()
        {
            /*if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
            }*/
        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_Alt))
            {
                // Slap whiffs
                if (sumoStatePrevious == SumoState.Slap || sumoStatePrevious == SumoState.Idle)
                {

                    SoundByte.PlayOneShotGame("sumoBrothers/whiff");

                    if (lookingAtCamera) {
                        sumoBrotherPHead.DoScaledAnimationAsync("SumoPSlapLook");
                    } else {
                        sumoBrotherPHead.DoScaledAnimationAsync("SumoPSlap");
                    }

                
                    if (sumoSlapDir == 2) {
                    sumoBrotherP.DoScaledAnimationAsync("SumoSlapToStomp", 0.5f);
                    } else if (sumoSlapDir == 1) {
                    sumoBrotherP.DoScaledAnimationAsync("SumoSlapFront", 0.5f);
                    } else {
                    sumoBrotherP.DoScaledAnimationAsync("SumoSlapBack", 0.5f);
                    }
                }
                // Stomp whiffs
                if (sumoStatePrevious == SumoState.Stomp && !sumoBrotherP.IsPlayingAnimationNames("SumoStompMiss"))
                {
                    SoundByte.PlayOneShotGame("sumoBrothers/miss");

                    inuSensei.DoScaledAnimationAsync("InuFloatMiss", 0.5f);

                    sumoBrotherP.DoScaledAnimationAsync("SumoStompMiss", 0.5f);
                    sumoBrotherPHead.DoScaledAnimationAsync("SumoPMiss");
                }
            }

        }
        
        public override void OnLateBeatPulse(double beat)
        {
                if (allowBopInu)
                {
                    if (goBopInu)
                    {
                        inuSensei.DoScaledAnimationAsync("InuBop", 0.5f);
                    }
                    else
                    {
                    //    inuSensei.DoScaledAnimationAsync("InuIdle", 0.5f);
                    }

                }

                if (allowBopSumo)
                {
                    if(goBopSumo)
                    {
                        BrosBop();
                    }
                    else
                    {
                    //    sumoBrotherP.DoScaledAnimationAsync("SumoIdle", 0.5f);
                    //    sumoBrotherG.DoScaledAnimationAsync("SumoIdle", 0.5f);
                    }
                    
                }

                print("current sumo state: " + sumoState + " and previous sumo state: " + sumoStatePrevious);
                print("sumo pose type: " + sumoPoseType);
        }

        public void Bop(double beat, float length, bool inu, bool sumo, bool inuAuto, bool sumoAuto)
        {
            goBopInu = inuAuto;
            goBopSumo = sumoAuto;

            if (inu || sumo)
            {
                List<BeatAction.Action> bops = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    bops.Add(new BeatAction.Action(beat + i, delegate
                    {
                        if (inu)
                        {
                            inuSensei.DoScaledAnimationAsync("InuBop", 0.5f);
                        }
                        if (sumo)
                        {
                            BrosBop();
                        }
                    }));
                }
                BeatAction.New(instance, bops);
            }

        }

        private void BrosBop()
        {
            if (sumoStatePrevious == SumoState.Idle) {
                sumoBrotherP.DoScaledAnimationAsync("SumoBop", 0.5f);
                sumoBrotherG.DoScaledAnimationAsync("SumoBop", 0.5f);
                sumoBrotherPHead.DoScaledAnimationAsync("SumoPIdle", 0.5f);
                sumoBrotherGHead.DoScaledAnimationAsync("SumoGIdle", 0.5f);
            } else if (sumoStatePrevious == SumoState.Pose) {
                sumoBrotherP.DoScaledAnimationAsync("SumoPosePBop" + sumoPoseTypeCurrent, 0.5f);
                sumoBrotherG.DoScaledAnimationAsync("SumoPoseGBop" + sumoPoseTypeCurrent, 0.5f);
            }

        }

        public void StompSignal(double beat, bool mute, bool lookatcam)
        {     
            if (sumoState == SumoState.Stomp || cueCurrentlyActive)
            {
                return;
            }

            CueRunning(beat + 3);
            sumoStompDir = true;

            if (lookatcam && sumoState == SumoState.Slap) {
                lookingAtCamera = true;
                sumoBrotherPHead.DoScaledAnimationAsync("SumoPSlapLook", 0.5f);
                sumoBrotherGHead.DoScaledAnimationAsync("SumoGSlapLook", 0.5f);

            }

            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat + 3, delegate { allowBopSumo = false; })
                });

            if (mute == false)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { allowBopInu = false; }),
                    new BeatAction.Action(beat, delegate { inuSensei.DoScaledAnimationAsync("InuBeatChange", 0.5f); }),
                    new BeatAction.Action(beat + 2, delegate { inuSensei.DoScaledAnimationAsync("InuBeatChange", 0.5f); })
                });

                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("sumoBrothers/stompsignal", beat),
                    new MultiSound.Sound("sumoBrothers/stompsignal", beat + 2f)
                }, forcePlay: true);
            
            }

            sumoStatePrevious = sumoState;
            sumoState = SumoState.Stomp;

            int firstStomp = 0;

            if (sumoStatePrevious == SumoState.Slap) {
                firstStomp = 1;
            } else if (sumoStatePrevious == SumoState.Pose) {
                firstStomp = 2;
            }

            StompRecursive(beat + 3, 1, firstStomp);
            
        }

        private void StompRecursive(double beat, double remaining, int firstStomp)
        {

            if (sumoState != SumoState.Stomp) { remaining -= 1; }

            if (remaining <= 0) { return; }

            if (firstStomp == 1) { // Stomp Animation - Transition from Slapping to Stomping
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 1, delegate { sumoStatePrevious = SumoState.Stomp; }),
                    new BeatAction.Action(beat + 1, delegate { lookingAtCamera = false; }),
                    new BeatAction.Action(beat + 1, delegate { sumoBrotherG.DoScaledAnimationAsync("SumoStompL", 0.5f); }),
                    new BeatAction.Action(beat + 1, delegate { sumoBrotherGHead.DoScaledAnimationAsync("SumoGStomp", 0.5f); }),
                    new BeatAction.Action(beat + 1, delegate { if (sumoState == SumoState.Stomp && !inuSensei.IsPlayingAnimationNames("InuFloatMiss")) {inuSensei.DoScaledAnimationAsync("InuFloat", 0.5f);} })                 
                });
            } else if (firstStomp == 2) { // Stomp Animation - Transition from Posing to Stomping
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { sumoBrotherP.DoScaledAnimationAsync("SumoPoseSwitch",0.5f); }),
                    new BeatAction.Action(beat, delegate { sumoBrotherG.DoScaledAnimationAsync("SumoPoseSwitch",0.5f); }),
                    new BeatAction.Action(beat, delegate { sumoBrotherPHead.DoScaledAnimationAsync("SumoPStomp",0.5f); }),
                    new BeatAction.Action(beat, delegate { sumoBrotherGHead.DoScaledAnimationAsync("SumoGStomp",0.5f); }),
                    new BeatAction.Action(beat + 1, delegate { sumoStatePrevious = SumoState.Stomp; }),
                    new BeatAction.Action(beat + 1, delegate { sumoBrotherG.DoScaledAnimationAsync("SumoStompR", 0.5f); }),
                    new BeatAction.Action(beat + 1, delegate { sumoBrotherGHead.DoScaledAnimationAsync("SumoGStomp", 0.5f); }),
                    new BeatAction.Action(beat + 1, delegate { if (sumoState == SumoState.Stomp && !inuSensei.IsPlayingAnimationNames("InuFloatMiss")) {inuSensei.DoScaledAnimationAsync("InuFloat", 0.5f);} })                 
                });
            } else if (sumoStompDir) { // Stomp Animation - Left Stomp
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { sumoStatePrevious = SumoState.Stomp; }),
                    new BeatAction.Action(beat, delegate { sumoBrotherP.DoScaledAnimationAsync("SumoStompPrepareL", 0.5f); }),
                    new BeatAction.Action(beat, delegate { sumoBrotherG.DoScaledAnimationAsync("SumoStompPrepareR", 0.5f); }),
                    new BeatAction.Action(beat, delegate { sumoBrotherPHead.DoScaledAnimationAsync("SumoPStomp", 0.5f); }),
                    new BeatAction.Action(beat, delegate { sumoBrotherGHead.DoScaledAnimationAsync("SumoGStomp", 0.5f); }),
                    new BeatAction.Action(beat + 1, delegate { sumoBrotherG.DoScaledAnimationAsync("SumoStompR", 0.5f); }),   
                    new BeatAction.Action(beat + 1, delegate { if (sumoState == SumoState.Stomp && !inuSensei.IsPlayingAnimationNames("InuFloatMiss")) {inuSensei.DoScaledAnimationAsync("InuFloat", 0.5f);} })             
                });
            } else { // Stomp Animation - Right Stomp
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { sumoStatePrevious = SumoState.Stomp; }),
                    new BeatAction.Action(beat, delegate { sumoBrotherP.DoScaledAnimationAsync("SumoStompPrepareR", 0.5f); }),
                    new BeatAction.Action(beat, delegate { sumoBrotherG.DoScaledAnimationAsync("SumoStompPrepareL", 0.5f); }),
                    new BeatAction.Action(beat, delegate { sumoBrotherPHead.DoScaledAnimationAsync("SumoPStomp", 0.5f); }),
                    new BeatAction.Action(beat, delegate { sumoBrotherGHead.DoScaledAnimationAsync("SumoGStomp", 0.5f); }),
                    new BeatAction.Action(beat + 1, delegate { sumoBrotherG.DoScaledAnimationAsync("SumoStompL", 0.5f); }),
                    new BeatAction.Action(beat + 1, delegate { if (sumoState == SumoState.Stomp && !inuSensei.IsPlayingAnimationNames("InuFloatMiss")) {inuSensei.DoScaledAnimationAsync("InuFloat", 0.5f);} })                       
                });
            }
        

        var stompInput = ScheduleInput(beat , 1, InputAction_BasicPress, StompHit, StompMiss, Nothing);
            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat, delegate { StompRecursive(beat + 2, remaining, 0); })
                });

            stompInput.IsHittable = () => {
                        return !sumoBrotherP.IsPlayingAnimationNames("SumoStompMiss");
                    };
                

            if (sumoStompDir) { sumoStompDir = false;}
            else
            { sumoStompDir = true;}
            print("sumo stomp dir: " + sumoStompDir);
            
        }

        public void SlapSignal(double beat, bool mute)
        {
            if (sumoState == SumoState.Slap || cueCurrentlyActive)
            {
                return;
            }

            CueRunning(beat + 3);

            sumoSlapDir = 0;

            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat + 3, delegate { allowBopSumo = false; }),
                new BeatAction.Action(beat + 3, delegate { sumoBrotherP.DoScaledAnimationAsync("SumoSlapPrepare",0.5f); }),
                new BeatAction.Action(beat + 3, delegate { sumoBrotherG.DoScaledAnimationAsync("SumoSlapPrepare", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate { sumoBrotherPHead.DoScaledAnimationAsync("SumoPSlap", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate { sumoBrotherGHead.DoScaledAnimationAsync("SumoGSlap", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate { if (sumoStatePrevious == SumoState.Pose) sumoBrotherP.DoScaledAnimationAsync("SumoPoseSwitch",0.5f); }),
                new BeatAction.Action(beat + 3, delegate { if (sumoStatePrevious == SumoState.Pose) sumoBrotherG.DoScaledAnimationAsync("SumoPoseSwitch",0.5f); }),
                new BeatAction.Action(beat + 3, delegate { if (sumoStatePrevious == SumoState.Pose) sumoBrotherPHead.DoScaledAnimationAsync("SumoPStomp",0.5f); }),
                new BeatAction.Action(beat + 3, delegate { if (sumoStatePrevious == SumoState.Pose) sumoBrotherGHead.DoScaledAnimationAsync("SumoGStomp",0.5f); })
                });

            if (mute == false)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat, delegate { allowBopInu = false; }),
                new BeatAction.Action(beat, delegate { inuSensei.DoScaledAnimationAsync("InuBeatChange", 0.5f); }),
                new BeatAction.Action(beat + 1, delegate { inuSensei.DoScaledAnimationAsync("InuBeatChange", 0.5f); }),
                new BeatAction.Action(beat + 2, delegate { inuSensei.DoScaledAnimationAsync("InuBeatChange", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate { inuSensei.DoScaledAnimationAsync("InuBeatChange", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate { allowBopInu = true; })
                });

                MultiSound.Play(new MultiSound.Sound[]
                {
                new MultiSound.Sound("sumoBrothers/slapsignal", beat),
                new MultiSound.Sound("sumoBrothers/slapsignal", beat + 1f),
                new MultiSound.Sound("sumoBrothers/slapsignal", beat + 2f),
                new MultiSound.Sound("sumoBrothers/slapsignal", beat + 3f)
                }, forcePlay: true);
            }

            sumoStatePrevious = sumoState;
            sumoState = SumoState.Slap;
            SlapRecursive(beat + 4, 4);

            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat + 4, delegate { sumoStatePrevious = SumoState.Slap; })
                });

        }

        private void SlapRecursive(double beat, double remaining)
        {

            if (sumoState != SumoState.Slap) {remaining -= 1; }

            if (remaining <= 0) { return; }

            if (remaining == 1 && sumoState == SumoState.Stomp) {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat - 0.5, delegate { sumoSlapDir = 2; })
                });
                }

            ScheduleInput(beat - 1, 1, InputAction_BasicPress, SlapHit, SlapMiss, Nothing);
            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat, delegate { SlapRecursive(beat + 1, remaining); })
                });
            
            
            
            print("the sumo slap direction:" + sumoSlapDir);
        }

        public void Crouch(double beat, float length, bool inu, bool sumo)
        {
            if (sumoStatePrevious == SumoState.Idle) {
                
                if (inu) { allowBopInu = false; inuSensei.DoScaledAnimationAsync("InuCrouch", 0.5f); }
                if (sumo) { sumoBrotherP.DoScaledAnimationAsync("SumoCrouch", 0.5f); sumoBrotherG.DoScaledAnimationAsync("SumoCrouch", 0.5f); 
                            allowBopSumo = false;}
                            BeatAction.New(instance, new List<BeatAction.Action>() {
            new BeatAction.Action(beat + length, delegate { if (sumoStatePrevious == SumoState.Idle) allowBopInu = true; }),
            new BeatAction.Action(beat + length, delegate { if (sumoStatePrevious == SumoState.Idle) allowBopSumo = true; }),
            new BeatAction.Action(beat + length, delegate { if (goBopSumo && sumoStatePrevious == SumoState.Idle) BrosBop(); }),
            new BeatAction.Action(beat + length, delegate { if (goBopInu && sumoStatePrevious == SumoState.Idle) inuSensei.DoScaledAnimationAsync("InuBop", 0.5f); })
            });
            }
        }

        public void LookAtCamera(double beat, float length)
        {
            if (sumoState == SumoState.Slap) {
            BeatAction.New(instance, new List<BeatAction.Action>() {
            new BeatAction.Action(beat, delegate { lookingAtCamera = true; }),
            new BeatAction.Action(beat, delegate { sumoBrotherPHead.DoScaledAnimationAsync("SumoPSlapLook", 0.5f); }),
            new BeatAction.Action(beat, delegate { sumoBrotherGHead.DoScaledAnimationAsync("SumoGSlapLook", 0.5f); }),
            new BeatAction.Action(beat, delegate { print("look"); }),
            new BeatAction.Action(beat + length, delegate { lookingAtCamera = false; }),
            new BeatAction.Action(beat + length, delegate { if (sumoState == SumoState.Slap) sumoBrotherPHead.DoScaledAnimationAsync("SumoPSlap", 0.5f); }),
            new BeatAction.Action(beat + length, delegate { if (sumoState == SumoState.Slap) sumoBrotherGHead.DoScaledAnimationAsync("SumoGSlap", 0.5f); }),
            new BeatAction.Action(beat + length, delegate { print("lookun"); })
            });
            }
        }

        public void EndPose(double beat, bool randomPose, int poseType, int backgroundType, bool confetti)
        {
            if (cueCurrentlyActive)
            { return; }

            CueRunning(beat + 3.5);
            sumoStatePrevious = sumoState;
            sumoState = SumoState.Pose;

            if (sumoPoseTypeNext > 0 & randomPose) {
                poseType = UnityEngine.Random.Range(1, 2);
                if (poseType >= sumoPoseTypeNext) poseType++;
            } else if (randomPose) {
                poseType = UnityEngine.Random.Range(1, 3);
            }

            var cond = Conductor.instance;

            ScheduleInput(beat, 4f, InputAction_Alt, PoseHit, PoseMiss, Nothing);

            var tweet = SoundByte.PlayOneShotGame("sumoBrothers/posesignal", -1, 1f, 1f, true);
            tweet.SetLoopParams(beat + 3, 0.05f);

            BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate { allowBopInu = false; }),
                new BeatAction.Action(beat, delegate { inuSensei.DoScaledAnimationAsync("InuAlarm", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate { allowBopInu = true; }),
                new BeatAction.Action(beat + 3, delegate { inuSensei.DoScaledAnimationAsync("InuIdle", 0.5f);; }),
                new BeatAction.Action(beat + 3, delegate { if (goBopInu == true) inuSensei.DoScaledAnimationAsync("InuBop", 0.5f); }),
                new BeatAction.Action(beat + 3.5, delegate { allowBopSumo = false; }),
                new BeatAction.Action(beat + 3.5, delegate { sumoPoseTypeNext = poseType; }),
                new BeatAction.Action(beat + 4, delegate { sumoStatePrevious = SumoState.Pose; }),
                new BeatAction.Action(beat + 4.5, delegate { allowBopSumo = true; })
            });

        }

        public void CueRunning(double beat)
        {
            cueCurrentlyActive = true;
            cueCurrentlyActiveBeat = beat;

            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat, delegate { cueCurrentlyActive = false; })
                });
        }

        void PoseHit(PlayerActionEvent caller, float state)
        {
            sumoPoseTypeCurrent = sumoPoseTypeNext.ToString();
            sumoPoseType = sumoPoseTypeNext;

            sumoBrotherP.DoScaledAnimationAsync("SumoPoseP" + sumoPoseTypeCurrent, 0.5f);
            sumoBrotherG.DoScaledAnimationAsync("SumoPoseG" + sumoPoseTypeCurrent, 0.5f);
            sumoBrotherGHead.DoScaledAnimationAsync("SumoGPose" + sumoPoseTypeCurrent, 0.5f);

            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("sumoBrothers/tink");
                sumoBrotherPHead.DoScaledAnimationAsync("SumoPPoseBarely" + sumoPoseTypeCurrent, 0.5f);
                // Dust is not meant to show up on a barely, need to somehow code that eventually
                dust.DoScaledAnimationAsync("dustGone", 0.5f);
            }
            else
            {
                sumoBrotherPHead.DoScaledAnimationAsync("SumoPPose" + sumoPoseTypeCurrent, 0.5f);
            }
            SoundByte.PlayOneShotGame("sumoBrothers/pose");

        }

        void PoseMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("sumoBrothers/miss");

            sumoPoseType = sumoPoseTypeNext;
            sumoPoseTypeCurrent = "Miss" + sumoPoseTypeNext.ToString();

            sumoBrotherPHead.DoScaledAnimationAsync("SumoPPose" + sumoPoseType.ToString(), 0.5f);
            sumoBrotherGHead.DoScaledAnimationAsync("SumoGPose" + sumoPoseType.ToString(), 0.5f);
            sumoBrotherP.DoScaledAnimationAsync("SumoPoseP" + sumoPoseTypeCurrent, 0.5f);
            sumoBrotherG.DoScaledAnimationAsync("SumoPoseG" + sumoPoseTypeCurrent, 0.5f);
        }

        void SlapHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("sumoBrothers/tink");

                if (lookingAtCamera) {
                    sumoBrotherPHead.DoScaledAnimationAsync("SumoPSlapLookBarely");
                    sumoBrotherGHead.DoScaledAnimationAsync("SumoGSlapLook", 0.5f);
                } else {
                    sumoBrotherPHead.DoScaledAnimationAsync("SumoPSlapBarely");
                    sumoBrotherGHead.DoScaledAnimationAsync("SumoGSlap", 0.5f);
                }

            }
            else
            {

                if (lookingAtCamera) {
                    sumoBrotherPHead.DoScaledAnimationAsync("SumoPSlapLook");
                    sumoBrotherGHead.DoScaledAnimationAsync("SumoGSlapLook", 0.5f);
                } else {
                    sumoBrotherPHead.DoScaledAnimationAsync("SumoPSlap");
                    sumoBrotherGHead.DoScaledAnimationAsync("SumoGSlap", 0.5f);
                }

            }

            if (sumoSlapDir == 1) { sumoSlapDir = 0;}
            else if (sumoSlapDir == 0) { sumoSlapDir = 1;}

            SoundByte.PlayOneShotGame("sumoBrothers/slap");
            impact.DoScaledAnimationAsync("impact", 0.5f);
            
            if (sumoSlapDir == 2) {
                sumoBrotherP.DoScaledAnimationAsync("SumoSlapToStomp", 0.5f);
                sumoBrotherG.DoScaledAnimationAsync("SumoSlapToStomp", 0.5f);
            } else if (sumoSlapDir == 1) {
                sumoBrotherP.DoScaledAnimationAsync("SumoSlapFront", 0.5f);
                sumoBrotherG.DoScaledAnimationAsync("SumoSlapFront", 0.5f);
            } else {
                sumoBrotherP.DoScaledAnimationAsync("SumoSlapBack", 0.5f);
                sumoBrotherG.DoScaledAnimationAsync("SumoSlapBack", 0.5f);
            }


        }

        void SlapMiss(PlayerActionEvent caller)
        {

            if (sumoSlapDir == 1) { sumoSlapDir = 0;}
            else if (sumoSlapDir == 0) { sumoSlapDir = 1;}

            SoundByte.PlayOneShotGame("sumoBrothers/miss");
            if (sumoSlapDir == 2) {
                sumoBrotherG.DoScaledAnimationAsync("SumoSlapToStomp", 0.5f);
            } else if (sumoSlapDir == 1) {
                sumoBrotherG.DoScaledAnimationAsync("SumoSlapFront", 0.5f);
            } else {
                sumoBrotherG.DoScaledAnimationAsync("SumoSlapBack", 0.5f);
            }

            sumoBrotherP.DoScaledAnimationAsync("SumoSlapMiss", 0.5f);
            sumoBrotherPHead.DoScaledAnimationAsync("SumoPMiss");

            if (sumoState == SumoState.Slap) {
            inuSensei.DoScaledAnimationAsync("InuBopMiss", 0.5f);
            }


        }

        void StompHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("sumoBrothers/tink");
                sumoBrotherPHead.DoScaledAnimationAsync("SumoPStompBarely", 0.5f);
            }
            else
            {
                sumoBrotherPHead.DoScaledAnimationAsync("SumoPStomp", 0.5f);
            }
            SoundByte.PlayOneShotGame("sumoBrothers/stomp");

            if (sumoStompDir) 
            {
                sumoBrotherP.DoScaledAnimationAsync("SumoStompL", 0.5f);
            } else {
                sumoBrotherP.DoScaledAnimationAsync("SumoStompR", 0.5f);
            }
            
            /*BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate { allowBopInu = false; }),
                new BeatAction.Action(beat, delegate { inuSensei.DoScaledAnimationAsync("InuAlarm", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate { allowBopInu = true; }),
                new BeatAction.Action(beat + 3, delegate { inuSensei.DoScaledAnimationAsync("InuIdle", 0.5f);; }),
                new BeatAction.Action(beat + 3, delegate { if (goBopInu == true) inuSensei.DoScaledAnimationAsync("InuBop", 0.5f); }),
                new BeatAction.Action(beat + 5, delegate { allowBopSumo = true; })
            });*/

            

        }

        void StompMiss(PlayerActionEvent caller)
        {
            if (!sumoBrotherP.IsPlayingAnimationNames("SumoStompMiss"))
            {
            SoundByte.PlayOneShotGame("sumoBrothers/miss");

            inuSensei.DoScaledAnimationAsync("InuFloatMiss", 0.5f);

            sumoBrotherP.DoScaledAnimationAsync("SumoStompMiss", 0.5f);
            sumoBrotherPHead.DoScaledAnimationAsync("SumoPMiss");
            }


        }

        void Nothing(PlayerActionEvent caller) { }

    }
}
